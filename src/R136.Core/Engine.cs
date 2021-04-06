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
		public NextStep DoNext { get; private set; } = NextStep.ShowStartMessage;
		public InputSpecs CommandInputSpecs => Facilities.Configuration.CommandInputSpecs;

		public bool IsInitialized { get; private set; } = false;

		public void StartLoadEntities(string[] groupLabels)
		{
			foreach (string label in groupLabels)
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

			if (ContextServices == null)
				return false;

			Facilities.Logger.Services = ContextServices;

			ObjectDumper.WriteClassType = false;
			ObjectDumper.WriteBidirectionalReferences = false;

			var entityReader = ContextServices.GetService<IEntityReader>();
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
				Facilities.Logger.LogDebug<Engine>($"Exception while loading configuration: {e}");
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

				List<string> texts = new();

				Room playerRoom = _player!.CurrentRoom;
				AddRoomInformation(texts, playerRoom);

				string? wayLine = GetWayLine(playerRoom);
				if (wayLine != null)
					texts.Add(wayLine);

				if (!_player!.IsDark)
				{
					string? itemLine = GetItemLine(playerRoom);
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

		public Result ProgressAnimateStatus()
		{
			if (!ValidateStep(NextStep.ProgressAnimateStatus))
				return Result.Error(IncorrectNextStep);

			var presentAnimates = PresentAnimates;
			if (presentAnimates.Count == 0)
				return Result.Success();

			(var texts, bool isAnimateTriggered) = ProgressPresentAnimateStatuses(presentAnimates);

			DoNext = isAnimateTriggered ? NextStep.ShowRoomStatus : NextStep.RunCommand;

			return Result.Success(texts.ToArray(), isAnimateTriggered);
		}

		public Result Run(string input)
		{
			if (!ValidateStep(NextStep.RunCommand))
				return Result.Error(IncorrectNextStep);

			if (_player!.IsDead)
				return Result.EndRequested(GetTexts(TextID.PlayerDead));

			string[] terms = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);

			if (terms.Length == 0)
				return Result.Error(GetTexts(TextID.NoCommand));

			(var processor, var id, string? command, var findResult) = _processors!.FindByName(terms[0]);

			Result result = findResult switch
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

			return DoPostRunProcessing(Result.ContinueWrappedContinuationStatus(ContinuationKey, status, input, DoContinuation));

			Result DoContinuation(ContinuationStatus status, string input)
			{
				if (status.Number != null && _processors![(CommandProcessorID)status.Number] is IContinuable processor)
					return processor.Continue(status.InnerStatus!, input).WrapInputRequest(ContinuationKey, status.Number.Value);

				return Result.Error();
			}
		}
	}
}
