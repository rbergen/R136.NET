using R136.BuildTool.Tools;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace R136.BuildTool
{
	partial class Program
	{
		private static readonly JsonSerializerOptions entityDeserializerOptions;

		static Program()
		{
			entityDeserializerOptions = new()
			{
				ReadCommentHandling = JsonCommentHandling.Skip,
				AllowTrailingCommas = true
			};

			entityDeserializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
		}

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
