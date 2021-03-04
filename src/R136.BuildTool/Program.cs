using R136.BuildTool.Tools;
using System.Text.Json;

namespace R136.BuildTool
{
	partial class Program
	{
		private static readonly JsonSerializerOptions _entityDeserializerOptions = new()
		{
			ReadCommentHandling = JsonCommentHandling.Skip,
			AllowTrailingCommas = true
		};

		static void Main(string[] args)
		{
			var arguments = Arguments.Parse(args);

			if (arguments.ConfigFileName != null)
				RunAutomatic(arguments);

			else
				RunManual();

		}

	}
}
