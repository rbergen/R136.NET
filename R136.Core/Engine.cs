using R136.Entities;
using R136.Entities.Animates;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R136.Core
{
	public partial class Engine : IEngine
	{
		public IServiceProvider? Services { get; set; }
		public IStatusManager StatusManager => this;
		public bool Initialized { get; private set; } = false;

		private const string EngineNotInitialized = "Engine not initialized";
		private const string IncorrectNextStep = "Step inconsistent with DoNext";
		private const string ConfigurationLabel = "configuration";
		private const string CommandsLabel = "commands";
		private const string TextsLabel = "texts";
		private const string RoomsLabel = "rooms";
		private const string AnimatesLabel = "animates";
		private const string ItemsLabel = "items";

		private IReadOnlyDictionary<RoomID, Room>? _rooms;
		private IReadOnlyDictionary<ItemID, Item>? _items;
		private IReadOnlyDictionary<AnimateID, Animate>? _animates;
		private Player? _player;
		private CommandProcessorMap? _processors;
		private bool _treeHasBurned = false;
		private bool _isAnimateTriggered = false;

		public NextStep DoNext { get; private set; } = NextStep.ShowRoomStatus;
		public InputSpecs CommandInputSpecs => Facilities.Configuration.CommandInputSpecs;

		public async Task<bool> Initialize()
		{
			if (Initialized)
				return true;

			if (Services == null)
				return false;

			Facilities.Services = Services;

			var entityReader = (IEntityReader?)Services.GetService(typeof(IEntityReader));
			if (entityReader == null)
				return false;

			var configurationTask = entityReader.ReadEntity<Configuration>(ConfigurationLabel);
			var textsTask = entityReader.ReadEntity<TypedTextsMap<int>.Initializer[]>(TextsLabel);
			var commandsTask = entityReader.ReadEntity<CommandInitializer[]>(CommandsLabel);
			var roomsTask = entityReader.ReadEntity<Room.Initializer[]>(RoomsLabel);
			var animatesTask = entityReader.ReadEntity<Animate.Initializer[]>(AnimatesLabel);
			var itemsTask = entityReader.ReadEntity<Item.Initializer[]>(ItemsLabel);

			var configuration = await configurationTask;
			if (configuration != null)
				Facilities.Configuration = configuration;

			if (Facilities.TextsMap.TextsMapCount == 0)
				Facilities.TextsMap.LoadInitializers(await textsTask);

			if (_rooms == null)
			{
				var rooms = await roomsTask;
				if (rooms == null)
					return false;

				_rooms = Room.FromInitializers(rooms);
			}

			if (_animates == null)
			{
				var animates = await animatesTask;
				if (animates == null)
					return false;

				_animates = Animate.FromInitializers(animates);
			}

			if (_items == null)
			{
				var items = await itemsTask;
				if (items == null)
					return false;

				_items = Item.FromInitializers(items, _animates);
			}

			if (_processors == null)
			{
				var commands = await commandsTask;
				if (commands == null)
					return false;

				_processors = CommandProcessor.FromInitializers(commands, _items, _animates);
			}

			ObjectDumper.WriteClassType = false;
			ObjectDumper.WriteBidirectionalReferences = false;

			if (_player == null)
				_player = new Player(_rooms[Facilities.Configuration.StartRoom]);

			if (_animates[AnimateID.Tree] is Tree tree)
				tree.Burned += TreeHasBurned;

			if (_animates[AnimateID.PaperHatch] is ITriggerable paperHatch)
				_processors.LocationProcessor.PaperRouteCompleted += paperHatch.Trigger;


			RegisterTurnEndingNotifiees(_items.Values);
			RegisterTurnEndingNotifiees(_animates.Values);

			Initialized = true;
			return true;
		}

		public ICollection<string>? RoomStatus
		{
			get
			{
				if (!Initialized && !Initialize().Result)
					throw new InvalidOperationException(EngineNotInitialized);

				if (DoNext != NextStep.ShowRoomStatus)
					return null;

				List<string> texts = new List<string>();

				var playerRoom = _player!.CurrentRoom;
				texts.AddRangeIfNotNull(GetTexts(TextID.YouAreAt, "room", playerRoom.Name));

				if (IsDark)
					texts.AddRangeIfNotNull(GetTexts(TextID.TooDarkToSee));

				else
				{
					if (playerRoom.IsForest && _treeHasBurned)
						texts.AddRangeIfNotNull(GetTexts(TextID.BurnedForestDescription));

					else if (playerRoom.Description != null)
						texts.Add(playerRoom.Description);

					var itemLine = GetItemLine(playerRoom);
					if (itemLine != null)
						texts.Add(itemLine);
				}

				var wayLine = GetWayLine(playerRoom);
				if (wayLine != null)
					texts.Add(wayLine);

				DoNext = IsAnimatePresent ? NextStep.ProgressAnimateStatus : NextStep.RunCommand;

				return texts;
			}
		}

		public Result ProgressAnimateStatus()
		{
			if (!Initialized && !Initialize().Result)
				throw new InvalidOperationException(EngineNotInitialized);

			if (DoNext != NextStep.ProgressAnimateStatus)
				return Result.Error(IncorrectNextStep);

			var presentAnimates = PresentAnimates;
			if (presentAnimates.Count == 0)
				return Result.Success();

			var startRoom = CurrentRoom;

			var texts = new List<string>();
			foreach(var animate in presentAnimates)
			{
				if (texts.Count > 0)
					texts.Add(string.Empty);

				texts.AddRangeIfNotNull(animate.ProgressStatus());
			}

			DoNext = CurrentRoom != startRoom ? NextStep.ShowRoomStatus : NextStep.RunCommand;

			return Result.Success(texts);
		}

		public Result Run(string input)
		{
			if (!Initialized && !Initialize().Result)
				return Result.Error(EngineNotInitialized);

			if (DoNext != NextStep.RunCommand)
				return Result.Error(IncorrectNextStep);


			var terms = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

			if (terms.Length == 0)
				return Result.Error(GetTexts(TextID.NoCommand));

			_isAnimateTriggered = false;

			(var processor, var id, var command, var findResult) = _processors!.FindByName(terms[0]);

			var result = findResult switch
			{
				FindResult.Ambiguous => Result.Error(GetTexts(TextID.AmbiguousCommand)),
				FindResult.NotFound => Result.Error(GetTexts(TextID.InvalidCommand)),
				_ => processor!.Execute(id!.Value, command!, terms.Length == 1 ? null : terms[1], _player!)
			};

			DoPostRunProcessing(result);

			return result.WrapContinuationRequest(this);
		}

		public Result Continue(object statusData, string input)
		{
			if (!Initialized && !Initialize().Result)
				return Result.Error(EngineNotInitialized);

			if (DoNext != NextStep.RunCommand)
				return Result.Error(IncorrectNextStep);

			var result = Result.ContinueWrappedContinuationData(this, statusData, input);

			DoPostRunProcessing(result);

			return result.WrapContinuationRequest(this);
		}

		private enum TextID
		{
			NoCommand,
			InvalidCommand,
			AmbiguousCommand,
			YouAreAt,
			TooDarkToSee,
			BurnedForestDescription,
			ItemLineTexts,
			WayLineTexts
		}
	}
}
