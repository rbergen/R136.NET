using R136.BuildTool.Tools;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace R136.BuildTool
{
	partial class Program
	{
		private static readonly JsonSerializerOptions _entityDeserializerOptions;

		static Program()
		{
			_entityDeserializerOptions = new()
			{
				ReadCommentHandling = JsonCommentHandling.Skip,
				AllowTrailingCommas = true
			};

			_entityDeserializerOptions.Converters.Add(new JsonStringEnumConverter(allowIntegerValues: false));
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
