using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using R136.Shell.Tools;
using System;
using System.Collections.Generic;
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

			if (args.Any(a => a.EqualsAny(new string[] { "--help", "-h", "/h" }, StringComparison.OrdinalIgnoreCase)))
				return ShowHelp(services.GetRequiredService<IConfiguration>());

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

    --bps <Waarde>
        Zet de gesimuleerde bitsnelheid van de terminal op de aangegeven waarde, of schakel bitsnelheidsimulatie uit (off). Standaard is {bpsdefault}, minimum is {bpsminimum}.

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

    --bps <Value>
        Sets the simulated bps rate of the terminal to the indicated value, or turn off bps simulation (off). The default is {bpsdefault}, the minimum is {bpsminimum}.

    --help, -h
        Show usage information.";

			helpText = helpText.ReplacePlaceholders(new Dictionary<string, object>
			{
				{ "command", Path.GetFileName(Process.GetCurrentProcess().MainModule!.FileName ?? string.Empty) },
				{ "languages", string.Join(", ", configuration.GetSection(Constants.Languages).GetChildren().Select(cs => cs.Key)) },
				{ "defaultlanguage", configuration[Constants.DefaultLanguage] },
				{ "bpsdefault", Constants.BPSDefault },
				{ "bpsminimum", Constants.BPSMinimum }
			});

			Console.WriteLine(helpText);

			return 0;
		}
	}
}
