using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Primitives;
using Microsoft.JSInterop;
using R136.Core;
using R136.Interfaces;
using R136.Web.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R136.Web.Pages
{
	public partial class Play
	{
		private string input = string.Empty;
		private ContinuationStatus continuationStatus = null;
		private InputSpecs inputSpecs;
		private MarkupString? error = null;
		private bool hasEnded = false;
		private bool isPaused = false;
		private string statusText = string.Empty;
		private bool showGameStatusModal = false;
		private LinkedList<string> commandHistory = new();
		private LinkedListNode<string> currentHistoryCommand = null;
		private bool preventKeyDownDefault = false;

#pragma warning disable IDE0044 // Add readonly modifier
		private ElementReference focusElement;
#pragma warning restore IDE0044 // Add readonly modifier

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
		public string GameData { get; set; }

		private ConciseStatus ComposeStatus()
			=> new()
			{
				IsPaused = this.isPaused,
				EngineSnapshot = Engine.TakeSnapshot(),
				MarkupContentLog = ContentLog.TakeSnapshot(),
				ContinuationStatus = this.continuationStatus,
				Language = LanguageProvider.Language,
				InputSpecs = this.inputSpecs
			};

		private async Task ApplyStatus(ConciseStatus status)
		{
			this.isPaused = status.IsPaused;
			Engine.RestoreSnapshot(status.EngineSnapshot);
			ContentLog.RestoreSnapshot(status.MarkupContentLog);
			this.continuationStatus = status.ContinuationStatus;
			LanguageProvider.Language = status.Language;
			this.inputSpecs = status.InputSpecs;

            await Engine.SetEntityGroup(status.Language);

            await SaveSnapshot(Constants.R136EngineStorageKey, status.EngineSnapshot);
			await SaveSnapshot(Constants.ContentLogStorageKey, status.MarkupContentLog);
			await SaveSnapshot(Constants.ContinuationStatusStorageKey, status.ContinuationStatus);
			await SaveSnapshot(Constants.InputSpecsStorageKey, status.InputSpecs);
			await SaveSnapshot(Constants.IsPausedStorageKey, this.isPaused);
		}

		private void ShowGameStatus()
		{
			List<byte> bytes = new();
			ComposeStatus().AddBytes(bytes);

			this.statusText = Convert.ToBase64String(bytes.ToArray());
			this.showGameStatusModal = true;
		}

		private async Task StatusTextSubmitted(string text)
			=> await ProcessStatusText(text);

		private async Task<bool> ProcessStatusText(string text)
		{
			byte[] bytes = null;
			ConciseStatus status = new();

			try
			{
				bytes = Convert.FromBase64String(text.Trim());
			}
			catch (Exception) { }

			if (bytes == null || status.LoadBytes(bytes) == null || !status.IsLoaded)
			{
				this.error = (MarkupString)LanguageProvider.GetConfigurationValue(Constants.InvalidGameStatusError);
				return false;
			}

			await ApplyStatus(status);
			
			return true;
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			if (firstRender)
				await JSRuntime.InvokeVoidAsync("R136JS.adoptVerticalPadding", "content-block", "top-bar", "bottom-bar");

			try
			{
				await this.focusElement.FocusAsync();
			}
			catch { }
		}

		protected override async Task OnInitializedAsync()
		{
			await Engine.Initialize(LanguageProvider.Language);

			if ((string.IsNullOrEmpty(GameData) || !await ProcessStatusText(GameData)) && await LocalStorage.ContainsSavedGame())
			{
				if (await LoadSnapshot<Engine.Snapshot>(Constants.R136EngineStorageKey, Engine.RestoreSnapshot))
				{
					await LoadSnapshot<MarkupContentLog.Snapshot>(Constants.ContentLogStorageKey, ContentLog.RestoreSnapshot);
					await LoadSnapshot<ContinuationStatus>(Constants.ContinuationStatusStorageKey, snapshot => { this.continuationStatus = snapshot; return true; });
					await LoadSnapshot<InputSpecs>(Constants.InputSpecsStorageKey, snapshot => { this.inputSpecs = snapshot; return true; });
					await LoadSnapshot<bool>(Constants.IsPausedStorageKey, snapshot => { this.isPaused = snapshot; return true; });
				}
				else
					await RemoveSnapshots();
			}

			await CycleEngine();
		}

		private async Task<bool> LoadSnapshot<TEntity>(string storageKey, Func<TEntity, bool> loader)
		{
			if (!await LocalStorage.ContainKeyAsync(storageKey))
				return false;

			try
			{
				return loader(await LocalStorage.GetItemAsync<TEntity>(storageKey));
			}
			catch (Exception ex)
			{
				Console.Write($"Error while restoring {nameof(TEntity)} snapshot from {storageKey}: {ex}");
				await LocalStorage.RemoveItemAsync(storageKey);
				return false;
			}
		}

		private async Task LanguageChanged()
		{
			ContentLog.Add(ContentBlockType.LanguageSwitch, LanguageProvider.GetConfigurationValue(Constants.LanguageSwitchText));
			await Engine.SetEntityGroup(LanguageProvider.Language);
			await SaveSnapshots();
		}

		private async Task ProceedClickedAsync()
		{
			this.isPaused = false;
			await CycleEngine();
		}

		private void RestartClickedAsync()
		{
			this.hasEnded = false;
			NavigationManager.NavigateTo("/", true);
		}

		private async Task CycleEngine()
		{
			bool proceed = true;

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
						proceed = await ProcessResult(Engine.ProgressAnimateStatus(), ContentBlockType.AnimateStatus);

						break;

					case NextStep.RunCommand:
						this.inputSpecs = Engine.CommandInputSpecs;
						proceed = false;
						await SaveSnapshots();

						break;
				}
			}
		}

		private async Task SaveSnapshot<TEntity>(string storageKey, TEntity entity)
		{
			if (entity != null)
				await LocalStorage.SetItemAsync(storageKey, entity);
			else
				await LocalStorage.RemoveItemAsync(storageKey);
		}

		private async Task SaveSnapshots()
		{
			await SaveSnapshot(Constants.R136EngineStorageKey, Engine.TakeSnapshot());
			await SaveSnapshot(Constants.ContentLogStorageKey, ContentLog.TakeSnapshot());
			await SaveSnapshot(Constants.ContinuationStatusStorageKey, this.continuationStatus);
			await SaveSnapshot(Constants.InputSpecsStorageKey, this.inputSpecs);
			await SaveSnapshot(Constants.IsPausedStorageKey, this.isPaused);
		}

		private async Task RemoveSnapshots()
		{
			await LocalStorage.RemoveItemAsync(Constants.R136EngineStorageKey);
			await LocalStorage.RemoveItemAsync(Constants.ContentLogStorageKey);
			await LocalStorage.RemoveItemAsync(Constants.ContinuationStatusStorageKey);
			await LocalStorage.RemoveItemAsync(Constants.InputSpecsStorageKey);
			await LocalStorage.RemoveItemAsync(Constants.IsPausedStorageKey);
		}

		private void ProcessKeyDown(KeyboardEventArgs e)
        {
			error = null;

			switch (e.Key)
            {
			case "ArrowDown":
			case "Down":
				if (this.currentHistoryCommand == null)
					break;

				this.currentHistoryCommand = this.currentHistoryCommand.Next;
				this.input = this.currentHistoryCommand?.Value ?? string.Empty;

				preventKeyDownDefault = true;
				break;

			case "ArrowUp":
			case "Up":
				if (this.currentHistoryCommand == null)
					this.currentHistoryCommand = this.commandHistory.Last;
				else if (this.currentHistoryCommand.Previous != null)
					this.currentHistoryCommand = this.currentHistoryCommand.Previous;
				else
					break;

				if (this.currentHistoryCommand != null)
					this.input = this.currentHistoryCommand.Value;

				preventKeyDownDefault = true;
				break;
				
			default:
				preventKeyDownDefault = false;
				break;
			}
		}

		private async Task SubmitInput(EventArgs e)
		{
			this.error = null;
			if (this.input != string.Empty)
				this.commandHistory.AddLast(this.input);

			this.input = ApplyInputSpecs(this.input);

			Result result = this.continuationStatus != null
				? Engine.Continue(this.continuationStatus, this.input)
				: Engine.Run(this.input);

			if (result.IsError)
			{
				this.error = (MarkupString)(result.Message != StringValues.Empty ? result.Message.ToMarkupString() : "An unspecified error occurred");
				if (this.input != string.Empty)
					this.currentHistoryCommand = this.commandHistory.Last;
				return;
			}

			ContentLog.Add(ContentBlockType.Input, this.input);
			this.input = string.Empty;
			this.currentHistoryCommand = null;
			this.continuationStatus = null;

			if (await ProcessResult(result, ContentBlockType.RunResult))
				await CycleEngine();
		}

		private string ApplyInputSpecs(string text)
		{
			if (this.inputSpecs.IsLowerCase)
				text = text.ToLower();

			if (text.Length > this.inputSpecs.MaxLength)
				text = text[..^(this.inputSpecs.MaxLength - 1)];

			return text;
		}

		private async Task<bool> ProcessResult(Result result, ContentBlockType blockType)
		{
			switch (result.Code)
			{
				case ResultCode.InputRequested:
					ContentLog.Add(blockType, result.Code, result.Message);

					this.continuationStatus = result.ContinuationStatus;
					this.inputSpecs = result.InputSpecs;

					await SaveSnapshots();
					return false;

				case ResultCode.EndRequested:
					ContentLog.Add(blockType, result.Code, result.Message);
					this.error = null;
					this.continuationStatus = null;
					this.hasEnded = true;

					await RemoveSnapshots();
					return false;

				case ResultCode.Success:
				case ResultCode.Failure:
					ContentLog.Add(blockType, result.Code, result.Message);

					if (result.PauseRequested)
					{
						this.isPaused = true;
						await SaveSnapshots();
						return false;
					}

					return true;
			}

			return true;
		}
	}
}
