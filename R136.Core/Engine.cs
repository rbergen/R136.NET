using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using R136.Entities;
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

		private const string EngineNotInitialized = "Engine not initialized";
		private const string IncorrectNextStep = "Step inconsistent with DoNext";
		private const string ConfigurationLabel = "configuration";
		private const string PropertiesLabel = "properties";
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
		private readonly Dictionary<string, TypedEntityTaskCollection> _entityTaskMap = new Dictionary<string, TypedEntityTaskCollection>();

		public NextStep DoNext { get; private set; } = NextStep.ShowStartMessage;
		public InputSpecs CommandInputSpecs => Facilities.Configuration.CommandInputSpecs;

		public bool IsInitialized { get; private set; } = false;

		public void StartLoadEntities(string[] groupLabels)
		{
			foreach (var label in groupLabels)
			{
				var entities = LoadEntities(label);
				if (entities != null)
				_entityTaskMap[label] = entities;
			}
		}

		public async Task<bool> Initialize(string? groupLabel = null)
		{
			if (IsInitialized)
				return true;

			if (Services == null)
				return false;

			Facilities.Services = Services;

			ObjectDumper.WriteClassType = false;
			ObjectDumper.WriteBidirectionalReferences = false;

			var entityReader = Services.GetService<IEntityReader>();
			if (entityReader == null)
				return false;

			try
			{
				var configuration = await entityReader.ReadEntity<Configuration>(null, ConfigurationLabel);
				if (configuration != null)
					Facilities.Configuration = configuration;
			}
			catch (Exception e)
			{
				LogLine($"Exception while loading configuration: {e}");
				return false;
			}


			if (groupLabel != null)
				await SetEntityGroup(groupLabel, false);

			IsInitialized = true;
			return true;
		}

		public async Task<bool> SetEntityGroup(string label)
			=> await SetEntityGroup(label, true);

		public StringValues StartMessage
		{
			get
			{
				if (!ValidateStep(NextStep.ShowStartMessage))
					return StringValues.Empty;

				DoNext = NextStep.ShowRoomStatus;

				return GetTexts(TextID.StartMessage);
			}
		}

		public StringValues RoomStatus
		{
			get
			{
				if (!ValidateStep(NextStep.ShowRoomStatus))
					return StringValues.Empty;

				var texts = new List<string>();

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

				return texts.ToArray();
			}
		}

		public StringValues ProgressAnimateStatus()
		{
			if (!ValidateStep(NextStep.ProgressAnimateStatus))
				return StringValues.Empty;

			var presentAnimates = PresentAnimates;
			if (presentAnimates.Count == 0)
				return StringValues.Empty;

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

			return texts.ToArray();
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
				FindResult.Ambiguous => Result.Error(GetTexts(TextID.AmbiguousCommand, "command", input)),
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
					? processor.Continue(status.InnerStatus!, input).WrapInputRequest(ContinuationKey, (int)_processors![(CommandProcessorID)status.Number]!.ID)
					: Result.Error()
				));
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
