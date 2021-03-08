using R136.BuildTool.Rooms;
using R136.BuildTool.Tasks;
using R136.BuildTool.Tools;
using R136.Entities;
using R136.Entities.General;
using R136.Entities.Global;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace R136.BuildTool
{
	partial class Program
	{
		private const string IndentSection = "   ";

		private static void RunAutomatic(Arguments arguments)
		{
			Console.WriteLine($"{Tags.Info} Starting processing from configuration file {arguments.ConfigFileName}...");

			ConversionTask[]? tasks;
			try
			{
				string jsonString = File.ReadAllText(arguments.ConfigFileName!, Encoding.UTF8);
				tasks = JsonSerializer.Deserialize<ConversionTask[]>(jsonString, new JsonSerializerOptions()
				{
					AllowTrailingCommas = true,
					ReadCommentHandling = JsonCommentHandling.Skip,
				});
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"{Tags.Error} Error reading configuration file {arguments.ConfigFileName}, aborting: {e.Message}.");
				return;
			}

			if (tasks == null)
			{
				Console.WriteLine($"{Tags.Warning} No tasks read from configuration file {arguments.ConfigFileName}, so nothing to process.");
				return;
			}

			Console.WriteLine($"{Tags.Info} Read {tasks.Length} tasks from configuration file {arguments.ConfigFileName}. Starting task processing.");
			Console.WriteLine();

			var taskTags = new List<string>();

			foreach (var task in tasks)
			{
				Console.WriteLine($"{Tags.Info} Starting processing for directory {task.Directory}...");
				taskTags.Add(ExecuteTask(arguments.IsTest, IndentSection, task));
				Console.WriteLine();
			}

			(string tag, int errorCount, int warningCount) = GetCompoundTagResult(taskTags);

			Console.WriteLine($"{tag} Processing of {tasks.Length} tasks completed, of which {errorCount} with errors and {warningCount} with warnings.");

			if (arguments.ShouldWait)
			{
				Console.WriteLine();
				Console.WriteLine($"{Tags.Warning} Press any key to end.");

				Console.ReadKey();
			}
		}

		private static string ExecuteTask(bool readOnly, string indent, ConversionTask task)
		{
			if (task.Conversions == null)
			{
				Console.WriteLine($"{indent}{Tags.Warning} No conversions specified, skipping directory {task.Directory}.");
				return Tags.Warning;
			}

			var conversionTags = new List<string>();

			foreach (var conversion in task.Conversions)
			{
				var entity = conversion.Key;
				var fileNames = conversion.Value ?? new FileNames();

				if (fileNames.Input == null)
				{
					fileNames.Input = $"{entity.ToString().ToLower()}base.json";
					Console.WriteLine($"{indent}{Tags.Info} Setting {entity} input filename to {fileNames.Input}.");
				}

				if (fileNames.Output == null)
				{
					fileNames.Output = $"{entity.ToString().ToLower()}.json";
					Console.WriteLine($"{indent}{Tags.Info} Setting {entity} output filename to {fileNames.Output}.");
				}

				Console.WriteLine($"{indent}{Tags.Info} Starting conversion of {entity} from {fileNames.Input} to {fileNames.Output}...");
				conversionTags.Add(GetEntityProcessor(conversion.Key)(readOnly, $"{indent}{IndentSection}", entity, CombinePath(task.Directory, fileNames.Input), CombinePath(task.Directory, fileNames.Output)));
				Console.WriteLine();
			}

			(string taskTag, int errorCount, int warningCount) = GetCompoundTagResult(conversionTags);

			Console.WriteLine($"{indent}{taskTag} Processing for directory {task.Directory} completed, with {errorCount} error(s) and {warningCount} warning(s).");
			return taskTag;
		}


		private static (string tag, int errorCount, int warningCount) GetCompoundTagResult(IEnumerable<string> tags)
		{
			int errorCount = tags.Count(Tags.IsError);
			int warningCount = tags.Count(Tags.IsWarning);

			string tag = errorCount > 0 ? Tags.Error
				: warningCount > 0 ? Tags.Warning
				: Tags.Success;

			return (tag, errorCount, warningCount);
		}

		private static string CombinePath(string directory, string fileName)
			=> Path.Join(directory, fileName);

		private static string ProcessEntity<TEntity>(bool readOnly, string indent, Entity entity, string inputPath, string outputPath) where TEntity : class
			=> ProcessEntity<TEntity, TEntity>(readOnly, indent, entity, inputPath, outputPath, null);

		private static string ProcessRooms(bool readOnly, string indent, Entity entity, string inputPath, string outputPath)
		{
			return ProcessEntity<RoomData, Room.Initializer[]>(readOnly, indent, entity, inputPath, outputPath, MakeConnections);

			Room.Initializer[]? MakeConnections(string indent, RoomData data)
			{
				Console.WriteLine($"{indent}{Tags.Info} Setting up connections between {entity}.");
				return data.CompileConnections($"{indent}{IndentSection}");
			}
		}

		private static string ProcessEntity<TInput, TOutput>(bool readOnly, string indent, Entity entity, string inputPath, string outputPath, Func<string, TInput, TOutput?>? processor) 
			where TInput : class
			where TOutput : class
		{
			TInput? inputObject;

			try
			{
				inputObject = JsonSerializer.Deserialize<TInput>(File.ReadAllText(inputPath, Encoding.UTF8), _entityDeserializerOptions);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"{indent}{Tags.Error} Error while reading input file {inputPath}, skipping {entity}: {e.Message}.");
				return Tags.Error;
			}

			if (inputObject == null)
			{
				Console.WriteLine($"{indent}{Tags.Warning} File {inputPath} contains no {entity}, skipping conversion.");
				return Tags.Warning;
			}

			if (inputObject is ICollection collectionEntity)
				Console.WriteLine($"{indent}{Tags.Info} Read {collectionEntity.Count} {entity}.");
			else
				Console.WriteLine($"{indent}{Tags.Info} Read {entity}.");

			TOutput? outputObject;
			
			if (processor != null) 
				outputObject = processor(indent, inputObject);

			else 
				outputObject = inputObject as TOutput;

			if (outputObject == null)
			{
				Console.WriteLine($"{indent}{Tags.Error} Nothing to write, aborting processing of {entity}.");
				return Tags.Error;
			} 
			else if (readOnly)
			{
				Console.WriteLine($"{indent}{Tags.Info} Read-only run, so skipping write of {entity}.");
				return Tags.Success;
			}

			try
			{
				File.WriteAllText(outputPath, JsonSerializer.Serialize(outputObject), Encoding.UTF8);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"{indent}{Tags.Error} Error while writing output file {outputPath}, conversion of {entity} failed: {e.Message}.");
				return Tags.Error;
			}

			Console.WriteLine($"{indent}{Tags.Success} {entity} successfully converted.");
			return Tags.Success;
		}

		private static Func<bool, string, Entity, string, string, string> GetEntityProcessor(Entity entity)
			=> entity switch
			{
				Entity.Animates => ProcessEntity<Animate.Initializer[]>,
				Entity.Commands => ProcessEntity<CommandInitializer[]>,
				Entity.Items => ProcessEntity<Item.Initializer[]>,
				Entity.Properties => ProcessEntity<LayoutProperties>,
				Entity.Rooms => ProcessRooms,
				Entity.Texts => ProcessEntity<TypedTextsMap<int>.Initializer[]>,
				_ => throw new ArgumentException("Unknown entity", nameof(entity))
			};
	}
}
