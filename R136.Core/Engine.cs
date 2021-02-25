using R136.Entities;
using R136.Entities.Animates;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace R136.Core
{
	public partial class Engine : IEngine
	{
		public IServiceProvider? Services { get; set; }
		public IStatusManager StatusManager => this;

		private const string EngineNotInitialized = "Engine not initialized";
		private const string IncorrectNextStep = "Step inconsistent with DoNext";
		private const string ConfigurationLabel = "configuration";
		private const string CommandsLabel = "commands";
		private const string TextsLabel = "texts";
		private const string RoomsLabel = "rooms";
		private const string AnimatesLabel = "animates";
		private const string ItemsLabel = "items";
		private const string ContinuationKey = "x8KQUbtDPZwlzWT5AOeJ";

		private IReadOnlyDictionary<RoomID, Room>? _rooms;
		private IReadOnlyDictionary<ItemID, Item>? _items;
		private IReadOnlyDictionary<AnimateID, Animate>? _animates;
		private Player? _player;
		private CommandProcessorMap? _processors;
		private bool _hasTreeBurned = false;
		private Dictionary<string, Task<TypedEntityCollection?>> _entityTaskMap = new Dictionary<string, Task<TypedEntityCollection?>>();

		public NextStep DoNext { get; private set; } = NextStep.ShowStartMessage;
		public InputSpecs CommandInputSpecs => Facilities.Configuration.CommandInputSpecs;

		public bool IsInitialized { get; private set; } = false;


		private void StartLoadEntities(string[] groupLabels)
		{
			foreach (var label in groupLabels)
				_entityTaskMap[label] = LoadEntities(label);
		}

		private async Task<TypedEntityCollection?> LoadEntities(string groupLabel)
		{
			if (Services == null)
				return null;

			var entityReader = (IEntityReader?)Services.GetService(typeof(IEntityReader));
			if (entityReader == null)
				return null;

			var entityCollection = new TypedEntityCollection();
			entityCollection.Add(await entityReader.ReadEntity<Configuration>(groupLabel, ConfigurationLabel));
			entityCollection.Add(await entityReader.ReadEntity<TypedTextsMap<int>.Initializer[]>(groupLabel, TextsLabel));
			entityCollection.Add(await entityReader.ReadEntity<CommandInitializer[]>(groupLabel, CommandsLabel));
			entityCollection.Add(await entityReader.ReadEntity<Room.Initializer[]>(groupLabel, RoomsLabel));
			entityCollection.Add(await entityReader.ReadEntity<Animate.Initializer[]>(groupLabel, AnimatesLabel));
			entityCollection.Add(await entityReader.ReadEntity<Item.Initializer[]>(groupLabel, ItemsLabel));

			return entityCollection;
		}

		public async Task<bool> Initialize()
		{
			try
			{
				if (IsInitialized)
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

				LogLine("loading configuration... ");
				var configuration = await configurationTask;

				if (configuration != null)
				{
					Facilities.Configuration = configuration;
					LogLine("successfully loaded configuration");
				}

				LogLine($"loading texts...");
				if (Facilities.TextsMap.TextsMapCount == 0)
				{
					var texts = await textsTask;
					if (texts != null)
					{
						Facilities.TextsMap.LoadInitializers(texts);
						LogLine($"successfully loaded texts");
					}
					else
						LogLine("texts loading failed");
				}

				if (_rooms == null)
				{
					LogLine("loading rooms... ");
					var rooms = await roomsTask;
					if (rooms == null)
					{
						LogLine("rooms loading failed");
						return false;
					}
					_rooms = Room.CreateMap(rooms);
					LogLine("successfully loaded rooms");
				}

				if (_animates == null)
				{
					LogLine("loading animates... ");
					var animates = await animatesTask;
					if (animates == null)
					{
						LogLine("animates loading failed");
						return false;
					}

					_animates = Animate.CreateMap(animates);
					LogLine("successfully loaded rooms");
				}

				if (_items == null)
				{
					LogLine("loading items... ");
					var items = await itemsTask;
					if (items == null)
					{
						LogLine("items loading failed");
						return false;
					}

					_items = Item.CreateMap(items, _animates);
					LogLine("successfully loaded items");
				}

				if (_processors == null)
				{
					LogLine("loading commands... ");
					var commands = await commandsTask;
					if (commands == null)
					{
						LogLine("commands loading failed");
						return false;
					}

					_processors = CommandProcessor.CreateMap(commands, _items, _animates);
					LogLine("successfully loaded commands");
				}

				ObjectDumper.WriteClassType = false;
				ObjectDumper.WriteBidirectionalReferences = false;

				LogLine("creating player");

				if (_player == null)
					_player = new Player(_rooms[Facilities.Configuration.StartRoom]);

				LogLine("finalizing initialization");

				if (_animates[AnimateID.Tree] is Tree tree)
					tree.Burned += TreeHasBurned;

				if (_animates[AnimateID.PaperHatch] is ITriggerable paperHatch)
					_processors.LocationProcessor.PaperRouteCompleted += paperHatch.Trigger;

				RegisterTurnEndingNotifiees(_items.Values);
				RegisterTurnEndingNotifiees(_animates.Values);

				LogLine("initialization complete");
				IsInitialized = true;
				return true;
			}
			catch (Exception e)
			{
				LogLine($"Exception during initialization: {e}");
				return false;
			}
		}

		public ICollection<string>? StartMessage
		{
			get
			{
				if (!ValidateStep(NextStep.ShowStartMessage))
					return null;

				DoNext = NextStep.ShowRoomStatus;

				return GetTexts(TextID.StartMessage);
			}
		}

		public ICollection<string>? RoomStatus
		{
			get
			{
				if (!ValidateStep(NextStep.ShowRoomStatus))
					return null;

				List<string> texts = new List<string>();

				var playerRoom = _player!.CurrentRoom;
				texts.AddRangeIfNotNull(GetTexts(TextID.YouAreAt, "room", playerRoom.Name));

				if (IsDark)
					texts.AddRangeIfNotNull(GetTexts(TextID.TooDarkToSee));

				else
				{
					if (playerRoom.IsForest && _hasTreeBurned)
						texts.AddRangeIfNotNull(GetTexts(TextID.BurnedForestDescription));

					else if (playerRoom.Description != null)
						texts.Add(playerRoom.Description);
				}

				var wayLine = GetWayLine(playerRoom);
				if (wayLine != null)
					texts.Add(wayLine);

				if (!IsDark)
				{
					var itemLine = GetItemLine(playerRoom);
					if (itemLine != null)
					{
						texts.Add(string.Empty);
						texts.Add(itemLine);
					}
				}

				DoNext = IsAnimatePresent ? NextStep.ProgressAnimateStatus : NextStep.RunCommand;

				return texts;
			}
		}

		public ICollection<string>? ProgressAnimateStatus()
		{
			if (!ValidateStep(NextStep.ProgressAnimateStatus))
				return null;

			var presentAnimates = PresentAnimates;
			if (presentAnimates.Count == 0)
				return null;

			var startRoom = CurrentRoom;
			var texts = new List<string>();
			var isAnimateTriggered = false;
				
			foreach(var animate in presentAnimates)
			{
				if (animate.IsTriggered)
				{
					isAnimateTriggered = true;
					animate.ResetTrigger();
				}

				if (texts.Count > 0)
					texts.Add(string.Empty);

				texts.AddRangeIfNotNull(animate.ProgressStatus());
			}

			if (CurrentRoom != startRoom)
				DoNext = NextStep.ShowRoomStatus;
			else if (isAnimateTriggered)
				DoNext = NextStep.Pause;
			else 
				DoNext = NextStep.RunCommand;

			return texts.Count > 0 ? texts : null;
		}

		public Result Run(string input)
		{
			if (!ValidateStep(NextStep.RunCommand))
				return Result.Error(IncorrectNextStep);

			if (_player!.LifePoints == 0)
				return Result.EndRequested(GetTexts(TextID.PlayerDead));

			var terms = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

			if (terms.Length == 0)
				return Result.Error(GetTexts(TextID.NoCommand));

			(var processor, var id, var command, var findResult) = _processors!.FindByName(terms[0]);

			var result = findResult switch
			{
				FindResult.Ambiguous => Result.Error(GetTexts(TextID.AmbiguousCommand)),
				FindResult.NotFound => Result.Error(GetTexts(TextID.InvalidCommand)),
				_ => processor!.Execute(id!.Value, command!, terms.Length == 1 ? null : terms[1], _player!).WrapInputRequest(ContinuationKey, (int)processor!.ID)
			};

			return DoPostRunProcessing(result);
		}

		public Result Continue(ContinuationStatus status, string input)
		{
			if (!ValidateStep(NextStep.RunCommand))
				return Result.Error(IncorrectNextStep);

			return DoPostRunProcessing(Result.ContinueWrappedContinuationStatus(ContinuationKey, status, input,
				(status, input) =>
					(status.Number != null && _processors![(CommandProcessorID)status.Number] is IContinuable processor)
					? processor.Continue(status.InnerStatus!, input)
					: Result.Error()
				).WrapInputRequest(ContinuationKey));
		}

		public void EndPause()
			=> DoNext = NextStep.ShowRoomStatus;

		private enum TextID
		{
			NoCommand,
			InvalidCommand,
			AmbiguousCommand,
			StartMessage,
			YouAreAt,
			TooDarkToSee,
			BurnedForestDescription,
			ItemLineTexts,
			WayLineTexts,
			PlayerDead
		}
	}
}
