using R136.BuildTool.Tasks;
using R136.BuildTool.Tools;
using R136.Entities;
using R136.Entities.General;
using R136.Entities.Global;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace R136.BuildTool
{
	partial class Program
	{
		private static void RunAutomatic(Arguments arguments)
		{
			ConversionTask[]? tasks;
			try
			{
				var jsonString = File.ReadAllText(arguments.ConfigFileName!, Encoding.UTF8);
				tasks = JsonSerializer.Deserialize<ConversionTask[]>(jsonString, new JsonSerializerOptions()
				{
					AllowTrailingCommas = true,
					ReadCommentHandling = JsonCommentHandling.Skip,
				});
			}
			catch(Exception ex)
			{
				Console.Error.WriteLine($"Error reading configuration file {arguments.ConfigFileName}, aboring: {ex.Message}");
				return;
			}

			if (tasks == null)
			{
				Console.WriteLine($"No tasks read from configuration file {arguments.ConfigFileName}, so nothing to process.");
				return;
			}

			foreach (var task in tasks)
				ExecuteTask(task);
		}

		private static void ExecuteTask(ConversionTask task)
		{
			Console.WriteLine($"Starting processing for directory {task.Directory}.");

			if (task.Conversions == null)
			{
				Console.WriteLine($"  No conversions specified, skipping directory.");
				return;
			}

			foreach (var conversion in task.Conversions)
			{
				var entity = conversion.Key;
				var fileNames = conversion.Value ?? new FileNames();

				if (fileNames.Input == null)
				{
					Console.WriteLine($"  Setting {entity} input filename to {fileNames.Input}");
					fileNames.Input = $"{entity.ToString().ToLower()}base.json";
				}

				if (fileNames.Output == null)
				{
					Console.WriteLine($"  Setting {entity} output filename to {fileNames.Output}");
					fileNames.Output = $"{entity.ToString().ToLower()}.json";
				}

				GetEntityProcessor(conversion.Key)(CombinePath(task.Directory, fileNames.Input), CombinePath(task.Directory, fileNames.Output));
			}
		}

		private static string CombinePath(string directory, string fileName)
			=> Path.Join(directory, fileName);

		private static void ProcessEntity<TEntity>(string inputPath, string outputPath)
		{

		}

		private static Action<string, string> GetEntityProcessor(Entity entity)
			=> entity switch
			{
				Entity.Animates => ProcessEntity<Animate.Initializer[]>,
				Entity.Commands => ProcessEntity<CommandInitializer[]>,
				Entity.Items => ProcessEntity<Item.Initializer[]>,
				Entity.Properties => ProcessEntity<LayoutProperties>,
				Entity.Rooms => ProcessEntity<Room.Initializer[]>,
				Entity.Texts => ProcessEntity<TypedTextsMap<int>.Initializer[]>,
				_ => throw new ArgumentException("Unknown entity", nameof(entity))
			};
	}
}
