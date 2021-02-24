using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using R136.Core;
using R136.Interfaces;
using R136.Web.Tools;
using System;
using System.Threading.Tasks;

namespace R136.Web.Pages
{
	public partial class Play
	{
		private string _input = string.Empty;
		private object _continuationData = null;
		private InputSpecs _inputSpecs;
		private MarkupString? _error = null;
		private bool _ended = false;
		private bool _pauseBeforeRoomStatus = false;
		private bool _pause = false;

		[Inject] 
		public IEngine Engine { get; set; }

		[Inject]
		public MarkupContentLog ContentLog { get; set; }

		[Inject]
		public NavigationManager NavigationManager { get; set; }

		[Inject]
		public ILocalStorageService LocalStorage { get; set; }

		[Parameter]
		public string Action { get; set; }

		protected override async Task OnInitializedAsync()
		{
			bool result = false;

			if (await LocalStorage.ContainKeyAsync(Constants.R136EngineStorageKey))
			{
				try
				{
					result = Engine.RestoreSnapshot(await LocalStorage.GetItemAsync<R136.Core.Engine.Snapshot>(Constants.R136EngineStorageKey));
				}
				catch (Exception ex)
				{
					Console.Write($"Error while restoring snapshot from {Constants.R136EngineStorageKey}: {ex}");
				}

				if (result && await LocalStorage.ContainKeyAsync(Constants.ContentLogStorageKey))
				{
					try
					{
						Console.WriteLine(await LocalStorage.GetItemAsStringAsync(Constants.ContentLogStorageKey));
						ContentLog.RestoreSnapshot(await LocalStorage.GetItemAsync<MarkupContentLog.Snapshot>(Constants.ContentLogStorageKey));
					}
					catch (Exception ex)
					{
						Console.Write($"Error while restoring snapshot from {Constants.ContentLogStorageKey}: {ex}");
						await LocalStorage.RemoveItemAsync(Constants.ContentLogStorageKey);
					}
				}

				if (!result)
				{
					await LocalStorage.RemoveItemAsync(Constants.R136EngineStorageKey);
					await LocalStorage.RemoveItemAsync(Constants.ContentLogStorageKey);
				}
			}

  		await CycleEngine();
		}

		private async Task ProceedClickedAsync()
		{
			_pause = false;
			await CycleEngine();
		}

		private void RestartClickedAsync()
		{
			_ended = false;
			NavigationManager.NavigateTo("/", true);
		}

		private async Task CycleEngine()
		{
			var proceed = true;
			
			while (proceed)
			{
				switch (Engine.DoNext)
				{
					case NextStep.ShowStartMessage:
						ContentLog.Add(ContentBlockType.StartMessage, Engine.StartMessage);

						break;

					case NextStep.ShowRoomStatus:
						if (_pauseBeforeRoomStatus)
						{
							_pause = true;
							_pauseBeforeRoomStatus = false;
							proceed = false;
						}
						else
							ContentLog.Add(ContentBlockType.RoomStatus, Engine.RoomStatus);

						break;

					case NextStep.ProgressAnimateStatus:
						ContentLog.Add(ContentBlockType.AnimateStatus, Engine.ProgressAnimateStatus());

						break;

					case NextStep.RunCommand:
						_inputSpecs = Engine.CommandInputSpecs;
						StateHasChanged();
						proceed = false;

						await LocalStorage.SetItemAsync(Constants.R136EngineStorageKey, Engine.TakeSnapshot());
						await LocalStorage.SetItemAsync(Constants.ContentLogStorageKey, ContentLog.TakeSnapshot());

						break;
				}
			}
		}

		private async Task SubmitInput(EventArgs e)
		{
			_error = null;

			_input = ApplyInputSpecs(_input);

			var result = _continuationData != null
				? Engine.Continue(_continuationData, ApplyInputSpecs(_input))
				: Engine.Run(_input);

			if (result.IsError)
			{
				_error = (MarkupString)(result.Message != null ? result.Message.ToMarkupString() : "An unspecified error occurred");
				return;
			}

			ContentLog.Add(ContentBlockType.Input, _input);
			_input = string.Empty;

			if (await ProcessResult(result, ContentBlockType.RunResult))
			{
				if (Engine.DoNext == NextStep.ProgressAnimateStatus)
					_pauseBeforeRoomStatus = true;

				await CycleEngine();
			}
		}

		private string ApplyInputSpecs(string text)
		{
			if (_inputSpecs.IsLowerCase)
				text = text.ToLower();

			if (text.Length > _inputSpecs.MaxLength)
				text = text[..^(_inputSpecs.MaxLength - 1)];

			return text;
		}

		private async Task<bool> ProcessResult(Result result, ContentBlockType blockType)
		{
			switch (result.Code)
			{
				case ResultCode.InputRequested:
					ContentLog.Add(blockType, result.Code, result.Message);
					_continuationData = result.ContinuationStatus.Data;

					return false;

				case ResultCode.EndRequested:
					_error = null;
					_continuationData = null;
					_ended = true;

					await LocalStorage.RemoveItemAsync(Constants.R136EngineStorageKey);
					await LocalStorage.RemoveItemAsync(Constants.ContentLogStorageKey);

					return false;

				case ResultCode.Success:
				case ResultCode.Failure:
					ContentLog.Add(blockType, result.Code, result.Message);

					return true;
			}

			return true;
		}
	}
}
