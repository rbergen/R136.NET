using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.BuildTool.Tools
{
	class Arguments
	{
		public bool IsTest { get; set; }
		public string? ConfigFileName { get; set; } = null;

		public static Arguments Parse(string[] args)
		{
			var arguments = new Arguments();

			for (int i = 0; i < args.Length; i++)
			{
				var arg = args[i];
				if (arg == "--test" || arg == "-t")
					arguments.IsTest = true;

				else
					arguments.ConfigFileName = arg;
			}

			return arguments;
		}
	}
}
