using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using R136.Shell.Tools;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace R136.Shell
{
	class Program
	{
		static async Task<int> Main(string[] args)
		{
			var services = Environment.Setup(args);

			if (args.Any(a
				=> a.Equals("--help", StringComparison.InvariantCultureIgnoreCase)
					|| a.Equals("-h", StringComparison.InvariantCultureIgnoreCase)
					|| a.Equals("/h", StringComparison.InvariantCultureIgnoreCase)))
			{
				return ShowHelp(services.GetRequiredService<IConfiguration>());
			}

			return await new GameConsole(services).Play();
		}

		private static int ShowHelp(IConfiguration configuration)
		{
			string helpText = configuration[Constants.LanguageParam] == Constants.Dutch
? @"Missiecode: R136
Copyright (c) R.I.P.

Gebruik: {command} [Opties...]

Opties:
    --lang <Code>
        Start R136 met de taal ingesteld op de aangegeven code. Beschikbare talen zijn {languages}, met {defaultlanguage} als standaard.

    --load yes|no
        Laad spelstatus indien beschikbaar (yes), of laad deze niet (no). Standaard is yes, wat betekent dat het spel zal verdergaan waar het was toen het de vorige keer werd afgebroken.

		--intro yes|no
        Laat bij een nieuw spel de intro-animatie en -tekst wel (yes) of niet (no) zien. Standaard is yes, wat betekent dat de intro wel wordt getoond.

		--baud <Waarde>
				Zet de gesimuleerde baudrate van de terminal op de aangegeven waarde.

    --help, -h, /h
        Toon gebruiksinformatie."

: @"Mission code: R136
Copyright (c) R.I.P.

Usage: {command} [Options...]

Options:
    --lang <Code>
        Start R136 with the language set to the indicated code. Available languages are {languages}, with {defaultlanguage} being the default.

    --load yes|no
        Load game status if available (yes), or don't load it (no). The default is yes, which means the game will continue where it got interrupted the last time it was run.

		--intro yes|no
        Shows the intro animation and text when starting a new game (yes), or doesn't (no). The default is yes, which means that the intro is shown.

		--baud <Value>
				Sets the simulated baud rate of the terminal to the indicated value.

    --help, -h
        Show usage information.";

			helpText = helpText.Replace("{command}", Path.GetFileName(Process.GetCurrentProcess().MainModule!.FileName));
			helpText = helpText.Replace("{languages}", string.Join(", ", configuration.GetSection(Constants.Languages).GetChildren().Select(cs => cs.Key)));
			helpText = helpText.Replace("{defaultlanguage}", configuration[Constants.DefaultLanguage]);

			Console.WriteLine(helpText);

			return 0;
		}
	}
}
