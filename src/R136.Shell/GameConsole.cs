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

			var proceed = true;

			while(proceed)
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
						SaveStatus(_engine.CommandInputSpecs);

						proceed = RunCommand();

						break;
					case NextStep.Pause:
						SaveStatus(null);

						Console.ReadKey();

						break;
				}
			}

			return Success;
		}

		private bool RunCommand()
		{
			while (true)
			{
				var input = GetInput();

				if (InputSpecs?.IsLowerCase ?? false)
					input = input.ToLower();

				var result = ContinuationStatus != null
					? _engine!.Continue(ContinuationStatus, input)
					: _engine!.Run(input);

				if (result.IsError)
				{
					WriteTexts(result.Message != StringValues.Empty ? result.Message : "An unspecified error occurred");
					continue;
				}

				ContinuationStatus = null;

				ProcessResult(result);

				if (result.IsEndRequest)
					return false;

				else if (result.IsEndRequest)
					continue;

				else
					return true;
			}
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
			Console.WriteLine($"Enter input (max length: {_status?.InputSpecs?.MaxLength}): ");
			var input = Console.ReadLine() ?? string.Empty;

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
		}

		private static void WriteTexts(StringValues texts)
		{
			Console.WriteLine(texts.ToPlainText());
		}
	}
}
