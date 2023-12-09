using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using R136.Core;
using R136.Interfaces;
using R136.Shell.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R136.Shell
{
	partial class GameConsole
	{
		private const int Success = 0;
		private const int Error = -1;

		private readonly IServiceProvider services;
		private readonly IConfiguration configuration;
		private readonly ILanguageProvider? languages;
		private readonly IEngine engine;
		private Status? status;
		private readonly Queue<string> texts = new();
		private readonly List<StringValues> messages = [];

		public GameConsole(IServiceProvider services)
		{
			this.services = services;
			this.configuration = this.services.GetRequiredService<IConfiguration>();
			this.languages = this.services.GetService<ILanguageProvider>();
			this.status = this.services.GetService<Status>();
			this.engine = this.services.GetRequiredService<IEngine>();
		}

		private InputSpecs? InputSpecs
		{
			get => this.status?.InputSpecs;
			set
			{
				this.status ??= new();
				this.status.InputSpecs = value;
			}
		}

		private ContinuationStatus? ContinuationStatus
		{
			get => this.status?.ContinuationStatus;
			set
			{
				this.status ??= new();
				this.status.ContinuationStatus = value;
			}
		}

		public async Task<int> Play()
		{
			Initialize();

			string language = this.languages?.Language ?? Constants.Dutch;

			var task = this.engine.Initialize(language);

			HandleIntro(language);

			await task;

			this.engine.CommandCallbacks = this;
			RestoreStatus();

			await RunEngineLoop();

			return Success;
		}

		private void HandleIntro(string language)
		{
			if (!(this.status?.IsLoaded ?? false) && this.configuration?[Constants.IntroParam] != Constants.ParamNo)
			{
				new Animation().Run();

				WritePlainText(this.introTexts[language]);
				WaitForKey();
			}

			Console.Clear();
		}

		private async Task RunEngineLoop()
		{
			bool firstRun = true;
			bool proceed = true;

			while (proceed)
			{
				switch (this.engine.DoNext)
				{
					case NextStep.ShowStartMessage:
						this.messages.AddIfNotEmpty(this.engine.StartMessage);

						break;

					case NextStep.ShowRoomStatus:
						this.messages.AddIfNotEmpty(this.engine.RoomStatus);

						break;

					case NextStep.ProgressAnimateStatus:
						ProcessResult(this.engine.ProgressAnimateStatus());

						break;

					case NextStep.RunCommand:
						WriteMessages();
						if (!firstRun)
							SaveStatus(this.engine.CommandInputSpecs);

						proceed = await RunCommand();

						break;
				}

				firstRun = false;
			}
		}

		private async Task<bool> RunCommand()
		{
			while (true)
			{
				string input = GetInput(out var inputLineLength);

				if (await ApplyLanguageChange(input))
					continue;

				input = ApplyInputSpecs(input);

				Result result = ContinuationStatus != null
					? this.engine!.Continue(ContinuationStatus, input)
					: this.engine!.Run(input);

				int top = Console.GetCursorPosition().Top;
				ClearLine(top - 1, inputLineLength);

				if (result.IsError)
				{
					ShowInputError(result, top);
					continue;
				}

				string finalInput = Constants.Prompt + input + '\n';
				BPSPrint(finalInput + Console.Out.NewLine);
				this.texts.Enqueue(finalInput);

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

		private void ShowInputError(Result result, int top)
		{
			ConsoleColor color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;

			var errorLine = Constants.ReversePrompt + (result.Message != StringValues.Empty ? result.Message : "An unspecified error occurred");
			BPSPrint(errorLine);

			Console.ForegroundColor = color;

			Console.ReadKey();

			ClearLine(top - 1, errorLine.Length + 1);
		}

		private string ApplyInputSpecs(string input)
		{
			if (InputSpecs?.IsLowerCase ?? false)
				input = input.ToLower();

			if (InputSpecs != null && input.Length > InputSpecs.MaxLength)
				input = input[..^(InputSpecs.MaxLength - 1)];

			return input;
		}

		private async Task<bool> ApplyLanguageChange(string input)
		{
			if (this.languages == null)
				return false;

			string[] segments = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (segments.Length != 2 || segments[0] != Constants.LanguageParam)
				return false;

			if ((this.languages.Language = segments[1]) != segments[1])
				return false;

			Console.Title = this.languages.GetConfigurationValue(Constants.TitleText) ?? Constants.TitleText;
			var result = this.engine!.SetEntityGroup(this.languages.Language);

			this.texts.Enqueue(Constants.Prompt + input);
			WriteText(new string[] {
				string.Empty,
				this.languages.GetConfigurationValue(Constants.LanguageSwitchText) ?? Constants.LanguageSwitchText,
				string.Empty
			});

			SaveStatus(this.status?.InputSpecs);

			await result;
			return true;
		}

		private void ProcessResult(Result result)
		{
			switch (result.Code)
			{
				case ResultCode.InputRequested:
					if (result.Message != StringValues.Empty)
						WriteMessage(result.Message);

					ContinuationStatus = result.ContinuationStatus;

					SaveStatus(result.InputSpecs);
					break;

				case ResultCode.EndRequested:
					this.messages.AddIfNotEmpty(result.Message);
					WriteMessages();

					this.status?.Remove();

					WaitForKey();
					break;

				case ResultCode.Success:
				case ResultCode.Failure:
					this.messages.AddIfNotEmpty(result.Message);

					if (result.PauseRequested)
					{
						WriteMessages();

						if (this.status != null)
							this.status.Pausing = true;
						
						SaveStatus(null);
						WaitForKey();
					}

					break;
			}
		}

		private readonly Dictionary<string, string> introTexts = new()
		{
			[Constants.Dutch] =
@"Terwijl je op het punt staat je avontuur te beginnen, denk je nog even na over waarom je hier, in deze verlaten, neertroostige omgeving staat.

Het verhaal begint op het moment dat je met drie andere wetenschappers een project begon over straling. In een vergevorderd stadium van het onderzoek werd er een fout gemaakt. In plaats van de gebruikelijke stoffen werden er andere, agressievere in de kernsplijter gebracht.
Het resultaat was even interessant als bedreigend: er ontstond een nieuwe straling, de positronenstraling. Deze straling heft elektronen op waardoor stoffen compleet in het niets verdwijnen. Een bepaald gedeelte van de reactor loste dan ook op in de lucht, en net op tijd kon een wereldramp voorkomen worden door het heldhaftig optreden van één van je collega's.
De betreffende wetenschapper werd even blootgesteld aan de straling, en na het gebeuren zonderde hij zich af.

Geschrokken door wat er gebeurde werd er besloten alles geheim te houden en het project te stoppen.
De wetenschapper die aan de straling was blootgesteld hield zich niet aan de afspraak en stal wat van de agressieve stof. Hij bouwde een bom, de positronenbom genaamd.

Hij vond dat de wereld de schuld had van zijn mutaties en hij wilde de wereld daarvoor laten boeten. Daarom verborg hij de bom, met een tijdmechanisme op een plaats die niemand zou durven betreden: de vallei der verderf.

Eén van de wetenschappers rook onraad en wilde de zaak gaan onderzoeken. 
Drie dagen later werd hij met een vleesmes in zijn rug op de stoep van zijn huis gevonden.
Toen zijn huis werd doorzocht stootte men op twee dingen: de plaats waar de bom lag en licht radioactieve voetstappen.
Jij en je collega begrepen wat er aan de hand was, en jullie besloten dat er moest worden ingegrepen. Aangezien je niet echt een held bent, werd er besloten dat de andere wetenschapper op pad zou gaan. Jij zou achterblijven om zijn reis te coördineren via een geheime radiofrequentie.
Je hebt nooit meer iets van hem gehoord.

Nu ben jij aan de beurt.

Je staat op de trap die naar de vallei leidt. Rechts van je staat een verweerd bordje: ""Betreden op eigen risico"". Je kijkt nog één keer achterom, en met trillende benen loop je naar beneden...
"
			,
			[Constants.English] =
@"As you're about to start your adventure, you think back to why you are here, standing in this desolate, depressing environment.

The story starts when you joined a radiation-related project with three other scientists. In an advanced stage of the research, a mistake was made. Instead of the usual matter, a different, more agressive one was introduced into the fission reactor.
The result was as interesting as it was threatening: a new radiation occurred, being positron radiation. This radiation dissolves electrons, due to which matter it comes in contact with completely dissolves into nothing. Part of the reacor dissolved into thin air and a global catastrophy could just be avoided by the heroic intervention of one of your colleagues.
The scientist in question was briefly exposed to the radiation, and he isolated himself after the event.

Shocked by what happened, it was decided to keep everything a secret and end the project.
The scientist who was exposed to the radiation did not stick to the agreement, and stole some of the aggressive matter. He built a new kind of bomb, a positron bomb. 

It was his opinion that the world was to blame for his mutations, and he wanted the world to pay for it. For that reason he hid the bomb with a time mechanism, in a place no one would dare to enter: the valley of perdition.

One of the scientists suspected malice and wanted to investigate the matter.
Three days later, he was found in front of his house with a butcher's knife in his back.
When his house was searched, two things were found: the location of the bomb, and low-level radioactive footsteps. 
Your colleague and you understood what was going on, and you decided that it was necessary to intervene. Because you're not really a hero, it was decided that the other scientist would make the trip. You would stay behind to coordinate his travels via a secret radio frequency.
You never heard from him again.

Now it's your turn.

You're on the stairs that lead to the valley. A weathered sign stands to the right of you: ""Enter at your own risk"". You look back one more time, and walk down with trembling legs...
"
		};
	}
}

