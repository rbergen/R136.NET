namespace R136.BuildTool.Tools
{
	class Arguments
	{
		public bool IsTest { get; set; }
		public bool ShouldWait { get; set; } = true;
		public string? ConfigFileName { get; set; } = null;

		public static Arguments Parse(string[] args)
		{
			Arguments arguments = new();

			foreach (string arg in args)
			{
				if (arg == "--test" || arg == "-t")
				{
					arguments.IsTest = true;
					continue;
				}

				if (arg == "--nowait")
				{
					arguments.ShouldWait = false;
					continue;
				}

				arguments.ConfigFileName = arg;
				break;
			}

			return arguments;
		}
	}
}
