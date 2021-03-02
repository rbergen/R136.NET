using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using R136.Core;
using R136.Interfaces;
using R136.Shell.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Shell
{
	class GameConsole
	{
		private const int Success = 0;
		private const int Error = -1;

		private readonly IServiceProvider _services;
		private readonly IConfiguration? _configuration;
		private readonly ILanguageProvider? _languages;
		private readonly IEngine? _engine;
		private Status? _status;
		private readonly Queue<string> _texts = new Queue<string>();

		public GameConsole(IServiceProvider services)
		{
			_services = services;
			_configuration = _services.GetService<IConfiguration>();
			_languages = _services.GetService<ILanguageProvider>();
			_status = _services.GetService<Status>();
			_engine = _services.GetService<IEngine>();
		}

		private InputSpecs? InputSpecs
		{
			get => _status?.InputSpecs;
			set
			{
				if (_status == null)
					_status = new Status();

				_status.InputSpecs = value;
			}
		}

		private ContinuationStatus? ContinuationStatus
		{
			get => _status?.ContinuationStatus;
			set
			{
				if (_status == null)
					_status = new Status();

				_status.ContinuationStatus = value;
			}
		}

		public async Task<int> Play()
		{
			if (_engine == null)
				return Error;

			await _engine.Initialize(_languages?.Language ?? Constants.Dutch);

			RestoreStatus();

			var firstRun = true;
			var proceed = true;

			while (proceed)
			{
				switch (_engine.DoNext)
				{
					case NextStep.ShowStartMessage:
						WriteTexts(_engine.StartMessage);

						break;

					case NextStep.ShowRoomStatus:
						WriteTexts(_engine.RoomStatus);

						break;

					case NextStep.ProgressAnimateStatus:
						WriteTexts(_engine.ProgressAnimateStatus());

						break;

					case NextStep.RunCommand:
						if (!firstRun) 
							SaveStatus(_engine.CommandInputSpecs);

						proceed = await RunCommand();

						break;
					case NextStep.Pause:
						SaveStatus(null);

						WriteLine(_languages?.GetConfigurationValue(Constants.ProceedText) ?? Constants.ProceedText);
						Console.Read();

						break;
				}

				firstRun = false;
			}

			return Success;
		}

		private void RestoreStatus()
		{
			if (_status?.EngineSnapshot != null && _engine != null)
				_engine.RestoreSnapshot(_status.EngineSnapshot);

			if (_status?.Texts != null)
			{
				_texts.Clear();
				foreach (var text in _status.Texts)
				{
					WriteLine(text);
					_texts.Enqueue(text);
				}
			}
		}

		private async Task<bool> RunCommand()
		{
			while (true)
			{
				var input = GetInput();

				if (await ApplyLanguageChange(input))
					continue;

				input = ApplyInputSpecs(input);

				var result = ContinuationStatus != null
					? _engine!.Continue(ContinuationStatus, input)
					: _engine!.Run(input);

				if (result.IsError)
				{
					Console.WriteLine();
					WriteTexts(result.Message != StringValues.Empty ? result.Message : "An unspecified error occurred");
					continue;
				}

				ContinuationStatus = null;

				ProcessResult(result);

				if (result.IsEndRequest)
					return false;

				else if (result.IsInputRequest)
					continue;

				else
					return true;
			}
		}

		private async Task<bool> ApplyLanguageChange(string input)
		{
			if (_languages == null)
				return false;

			var segments = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if ((segments?.Length ?? 0) == 2 && segments![0] == "lang")
			{
				_languages.Language = segments![1];
				var result = _engine!.SetEntityGroup(_languages.Language);
				WriteTexts(_languages.GetConfigurationValue(Constants.LanguageSwitchText));
				await result;

				return true;
			}

			return false;
		}

		private void ProcessResult(Result result)
		{
			switch (result.Code)
			{
				case ResultCode.InputRequested:
					WriteTexts(result.Message);

					ContinuationStatus = result.ContinuationStatus;

					SaveStatus(result.InputSpecs);
					break;

				case ResultCode.EndRequested:
					WriteTexts(result.Message);

					_status?.Remove();
					break;

				case ResultCode.Success:
				case ResultCode.Failure:
					WriteTexts(result.Message);

					break;
			}
		}

		private string GetInput()
		{
			Console.Write("> ");

			var input = Console.ReadLine() ?? string.Empty;
			_texts.Enqueue($"> {input}");

			Console.WriteLine();

			return input;
		}

		private string ApplyInputSpecs(string input)
		{
			if (InputSpecs?.IsLowerCase ?? false)
				input = input.ToLower();

			if (InputSpecs != null && input.Length > InputSpecs.MaxLength)
				input = input[..^(InputSpecs.MaxLength - 1)];

			return input;
		}

		private void SaveStatus(InputSpecs? inputSpecs)
		{
			if (_status == null)
				return;

			_status.InputSpecs = inputSpecs;
			_status.EngineSnapshot = _engine!.TakeSnapshot();
			_status.Texts = _texts.ToArray();

			_status.Save();
		}

		private void WriteTexts(StringValues texts)
		{
			if (StringValues.IsNullOrEmpty(texts))
				return;

			var plainText = texts.ToPlainText();
			WriteLine(plainText);
			_texts.Enqueue(plainText);
			while (_texts.Count > 20)
				_texts.Dequeue();
		}

		private static void WriteLine(string plainText)
		{
			var consoleWidth = Console.WindowWidth;

			foreach (var item in plainText.Split('\n'))
			{
				var plainLine = item;
				while (plainLine.Length >= consoleWidth)
				{
					var lastFittingSpace = plainLine.LastIndexOf(' ', consoleWidth - 1);
					if (lastFittingSpace <= 0)
						break;

					Console.WriteLine(plainLine[..(lastFittingSpace)]);
					plainLine = plainLine[(lastFittingSpace + 1)..];
				}

				Console.WriteLine(plainLine);
			}
		}
	}
}
