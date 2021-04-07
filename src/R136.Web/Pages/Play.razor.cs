﻿using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
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
		private string _input = string.Empty;
		private ContinuationStatus _continuationStatus = null;
		private InputSpecs _inputSpecs;
		private MarkupString? _error = null;
		private bool _hasEnded = false;
		private bool _isPaused = false;
		private string _statusText = string.Empty;
		private bool _showGameStatusModal = false;

#pragma warning disable IDE0044 // Add readonly modifier
		private ElementReference _focusElement;
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
				IsPaused = _isPaused,
				EngineSnapshot = Engine.TakeSnapshot(),
				MarkupContentLog = ContentLog.TakeSnapshot(),
				ContinuationStatus = _continuationStatus,
				Language = LanguageProvider.Language,
				InputSpecs = _inputSpecs
			};

		private async Task ApplyStatus(ConciseStatus status)
		{
			_isPaused = status.IsPaused;
			Engine.RestoreSnapshot(status.EngineSnapshot);
			ContentLog.RestoreSnapshot(status.MarkupContentLog);
			_continuationStatus = status.ContinuationStatus;
			LanguageProvider.Language = status.Language;
			_inputSpecs = status.InputSpecs;

			await SaveSnapshot(Constants.R136EngineStorageKey, status.EngineSnapshot);
			await SaveSnapshot(Constants.ContentLogStorageKey, status.MarkupContentLog);
			await SaveSnapshot(Constants.ContinuationStatusStorageKey, status.ContinuationStatus);
			await SaveSnapshot(Constants.InputSpecsStorageKey, status.InputSpecs);
			await SaveSnapshot(Constants.IsPausedStorageKey, _isPaused);
		}

		private void ShowGameStatus()
		{
			List<byte> bytes = new();
			ComposeStatus().AddBytes(bytes);

			_statusText = Convert.ToBase64String(bytes.ToArray());
			_showGameStatusModal = true;
		}

		private async Task StatusTextSubmitted(string text)
		{
			await ProcessStatusText(text);
		}

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
				_error = (MarkupString)LanguageProvider.GetConfigurationValue(Constants.InvalidGameStatusError);
				return false;
			}

			await ApplyStatus(status);
			
			return true;
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await JSRuntime.InvokeVoidAsync("R136JS.enableTooltips");

			try
			{
				await _focusElement.FocusAsync();
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
					await LoadSnapshot<ContinuationStatus>(Constants.ContinuationStatusStorageKey, snapshot => { _continuationStatus = snapshot; return true; });
					await LoadSnapshot<InputSpecs>(Constants.InputSpecsStorageKey, snapshot => { _inputSpecs = snapshot; return true; });
					await LoadSnapshot<bool>(Constants.IsPausedStorageKey, snapshot => { _isPaused = snapshot; return true; });
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
			_isPaused = false;
			await CycleEngine();
		}

		private void RestartClickedAsync()
		{
			_hasEnded = false;
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
						_inputSpecs = Engine.CommandInputSpecs;
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
			await SaveSnapshot(Constants.ContinuationStatusStorageKey, _continuationStatus);
			await SaveSnapshot(Constants.InputSpecsStorageKey, _inputSpecs);
			await SaveSnapshot(Constants.IsPausedStorageKey, _isPaused);
		}

		private async Task RemoveSnapshots()
		{
			await LocalStorage.RemoveItemAsync(Constants.R136EngineStorageKey);
			await LocalStorage.RemoveItemAsync(Constants.ContentLogStorageKey);
			await LocalStorage.RemoveItemAsync(Constants.ContinuationStatusStorageKey);
			await LocalStorage.RemoveItemAsync(Constants.InputSpecsStorageKey);
			await LocalStorage.RemoveItemAsync(Constants.IsPausedStorageKey);
		}

		private async Task SubmitInput(EventArgs e)
		{
			_error = null;

			_input = ApplyInputSpecs(_input);

			Result result = _continuationStatus != null
				? Engine.Continue(_continuationStatus, _input)
				: Engine.Run(_input);

			if (result.IsError)
			{
				_error = (MarkupString)(result.Message != StringValues.Empty ? result.Message.ToMarkupString() : "An unspecified error occurred");
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
					_inputSpecs = result.InputSpecs;

					await SaveSnapshots();
					return false;

				case ResultCode.EndRequested:
					ContentLog.Add(blockType, result.Code, result.Message);
					_error = null;
					_continuationStatus = null;
					_hasEnded = true;

					await RemoveSnapshots();
					return false;

				case ResultCode.Success:
				case ResultCode.Failure:
					ContentLog.Add(blockType, result.Code, result.Message);

					if (result.PauseRequested)
					{
						_isPaused = true;
						await SaveSnapshots();
						return false;
					}

					return true;
			}

			return true;
		}
	}
}
