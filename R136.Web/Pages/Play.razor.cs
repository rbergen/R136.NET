using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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
		private ContinuationStatus _continuationStatus = null;
		private InputSpecs _inputSpecs;
		private MarkupString? _error = null;
		private bool _ended = false;
		private bool _pause = false;

		[Parameter]
		public string Language { get; set; }

		[Inject]
		public IJSRuntime JSRuntime { get; set; }

		[Inject] 
		public IEngine Engine { get; set; }

		[Inject]
		public MarkupContentLog ContentLog { get; set; }

		[Inject]
		public NavigationManager NavigationManager { get; set; }

		[Inject]
		public ILocalStorageService LocalStorage { get; set; }

		[Inject]
		public ILanguageProvider LanguageProvider { get; set; }

		[Parameter]
		public string Action { get; set; }

		protected override async Task OnAfterRenderAsync(bool firstRender)
			=> await JSRuntime.InvokeAsync<bool>("stretchToHeight", "contentlog", "app");

		protected override async Task OnInitializedAsync()
		{
			Engine.Initialize();
			Engine.SetEntityGroup(LanguageProvider.Language);

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
						ContentLog.RestoreSnapshot(await LocalStorage.GetItemAsync<MarkupContentLog.Snapshot>(Constants.ContentLogStorageKey));
					}
					catch (Exception ex)
					{
						Console.Write($"Error while restoring snapshot from {Constants.ContentLogStorageKey}: {ex}");
						await LocalStorage.RemoveItemAsync(Constants.ContentLogStorageKey);
					}
				}

				if (result && await LocalStorage.ContainKeyAsync(Constants.ContinuationStatusStorageKey))
				{
					try
					{
						_continuationStatus = await LocalStorage.GetItemAsync<ContinuationStatus>(Constants.ContinuationStatusStorageKey);
					}
					catch (Exception ex)
					{
						Console.Write($"Error while restoring snapshot from {Constants.ContinuationStatusStorageKey}: {ex}");
						await LocalStorage.RemoveItemAsync(Constants.ContinuationStatusStorageKey);
					}
				}

				if (!result)
					await RemoveSnapshot();
			}

			await CycleEngine();
		}

		private void LanguageChanged()
			=> Engine.SetEntityGroup(LanguageProvider.Language);

		private async Task ProceedClickedAsync()
		{
			_pause = false;
			Engine.EndPause();
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
						ContentLog.Add(ContentBlockType.RoomStatus, Engine.RoomStatus);

						break;

					case NextStep.ProgressAnimateStatus:
						ContentLog.Add(ContentBlockType.AnimateStatus, Engine.ProgressAnimateStatus());

						break;

					case NextStep.RunCommand:
						_inputSpecs = Engine.CommandInputSpecs;
						proceed = false;
						await SaveSnapshot();

						break;
					case NextStep.Pause:
						_pause = true;
						proceed = false;
						await SaveSnapshot();

						break;
				}
			}
		}

		private async Task SaveSnapshot()
		{
			await LocalStorage.SetItemAsync(Constants.R136EngineStorageKey, Engine.TakeSnapshot());
			await LocalStorage.SetItemAsync(Constants.ContentLogStorageKey, ContentLog.TakeSnapshot());
			if (_continuationStatus != null)
				await LocalStorage.SetItemAsync(Constants.ContinuationStatusStorageKey, _continuationStatus);
			else
				await LocalStorage.RemoveItemAsync(Constants.ContinuationStatusStorageKey);

			Console.WriteLine(_continuationStatus);
			Console.WriteLine(await LocalStorage.GetItemAsStringAsync(Constants.ContinuationStatusStorageKey));
		}

		private async Task RemoveSnapshot()
		{
			await LocalStorage.RemoveItemAsync(Constants.R136EngineStorageKey);
			await LocalStorage.RemoveItemAsync(Constants.ContentLogStorageKey);
			await LocalStorage.RemoveItemAsync(Constants.ContinuationStatusStorageKey);
		}

		private async Task SubmitInput(EventArgs e)
		{
			_error = null;

			_input = ApplyInputSpecs(_input);

			var result = _continuationStatus != null
				? Engine.Continue(_continuationStatus, ApplyInputSpecs(_input))
				: Engine.Run(_input);

			if (result.IsError)
			{
				_error = (MarkupString)(result.Message.ToMarkupString() ?? "An unspecified error occurred");
				return;
			}

			ContentLog.Add(ContentBlockType.Input, _input);
			_input = string.Empty;
			_continuationStatus = null;

			if (await ProcessResult(result, ContentBlockType.RunResult))
				await CycleEngine();
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

					_continuationStatus = result.ContinuationStatus;

					await SaveSnapshot();
					return false;

				case ResultCode.EndRequested:
					ContentLog.Add(blockType, result.Code, result.Message);
					_error = null;
					_continuationStatus = null;
					_ended = true;

					await RemoveSnapshot();
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
