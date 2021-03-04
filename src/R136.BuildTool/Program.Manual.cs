using R136.BuildTool.Tools;
using R136.Entities;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace R136.BuildTool
{
	partial class Program
	{
		private static void RunManual()
		{
			ObjectDumper.WriteBidirectionalReferences = false;
			ObjectDumper.WriteClassType = false;

			ProcessEntity<Animate.Initializer[]>("Animates");
			ProcessCommands("Commands");
			ProcessItems("Items");
			ProcessEntity<LayoutProperties>("Layout properties");
			ProcessRooms("Rooms");
			ProcessTexts("Texts");
			ProcessConfiguration("Configuration");
		}

		private static string GetOutputPath(string name, string path)
		{
			string fileName = Path.GetFileNameWithoutExtension(path);

			if (fileName.Length > 4 && fileName.ToLower().EndsWith("base"))
				fileName = fileName[..^4] + ".json";
			else
				fileName += ".new.json";

			Console.Write($"*{name}* output filename, (Enter for '{fileName}', - cancels, .json added if omitted): ");
			string? userEntry = Console.ReadLine();

			if (userEntry == null || userEntry == "-")
				return string.Empty;

			if (!string.IsNullOrWhiteSpace(userEntry))
			{
				if (!userEntry.ToLower().EndsWith(".json"))
					userEntry += ".json";

				fileName = userEntry;
			}

			return Path.Combine(Path.GetDirectoryName(path)!, fileName);
		}

		private static bool Confirm(string prompt)
		{
			Console.Write($"{prompt} [y/N]: ");
			var input = Console.ReadLine();
			return input != null && input.Trim().ToLower() == "y";
		}

		private static (T? entity, string? path) ReadEntity<T>(string name) where T : class
		{
			Console.Write($"*{name}* JSON file path (Enter to skip): ");
			var input = Console.ReadLine();
			if (string.IsNullOrWhiteSpace(input))
				return (null, null);

			var jsonFilePath = input.Trim();

			try
			{
				var jsonString = File.ReadAllText(jsonFilePath, Encoding.UTF8);
				var entity = JsonSerializer.Deserialize<T>(jsonString, _entityDeserializerOptions);

				return (entity, jsonFilePath);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Error while reading JSON file: {e}");
				return (null, null);
			}
		}

		private static void Print<T>(T entity, string name)
		{
			if (!Confirm($"Print {name}?"))
				return;

			Console.WriteLine(ObjectDumper.Dump(entity));
			Console.WriteLine();
		}

		private static void ProcessEntity<T>(string name) where T : class
		{
			(var entity, var path) = ReadEntity<T>(name);

			if (entity == null || path == null)
				return;

			WriteEntity(entity, name, path);
		}

		private static void WriteEntity<T>(T entity, string name, string path) where T : class
		{
			path = GetOutputPath(name, path);
			if (path == string.Empty)
				return;

			try
			{
				string jsonString = JsonSerializer.Serialize(entity);
				File.WriteAllText(path, jsonString, Encoding.UTF8);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Error while writing JSON file: {e}");
				return;
			}

			Console.WriteLine($"{name} JSON file written to: {path}");
			Console.WriteLine();
		}

		private static void ProcessTexts(string name)
		{
			(var texts, var path) = ReadEntity<TypedTextsMap<int>.Initializer[]>(name);

			if (texts == null || path == null)
				return;

			var maps = new TypedTextsMap<int>();
			maps.LoadInitializers(texts);

			Console.WriteLine($"{name} map after load:");
			Console.WriteLine(ObjectDumper.Dump(maps));

			WriteEntity(texts, name, path);
		}

		private static void ProcessItems(string name)
		{
			(var items, var path) = ReadEntity<Item.Initializer[]>(name);

			if (items == null || path == null)
				return;

			var textsMap = new KeyedTextsMap<ItemID, Item.TextType>();
			foreach (var item in items)
			{
				textsMap.LoadInitializer(item.ID, new Item.TextsInitializer(Item.TextType.Use, item));
				textsMap.LoadInitializer(item.ID, new Item.TextsInitializer(Item.TextType.Combine, item));
			}
			Console.WriteLine($"{name} map after load:");
			Console.WriteLine(ObjectDumper.Dump(textsMap));

			WriteEntity(items, name, path);
		}

		private static void ProcessCommands(string name)
		{
			(var commands, var path) = ReadEntity<CommandInitializer[]>(name);

			if (commands == null || path == null)
				return;

			var textsMap = new KeyedTextsMap<CommandID, int>();
			foreach (var command in commands)
			{
				if (command.TextMap != null)
				{
					foreach (var mapping in command.TextMap)
						textsMap.LoadInitializer(command.ID, mapping);
				}
			}
			Console.WriteLine($"{name} map after load:");
			Console.WriteLine(ObjectDumper.Dump(textsMap));

			WriteEntity(commands, name, path);
		}

		private static void ProcessConfiguration(string name)
		{
			(var configuration, var jsonFilePath) = ReadEntity<Configuration>(name);

			if (configuration == null || jsonFilePath == null)
				return;

			Print(configuration, name);
		}

		private static void ProcessRooms(string name)
		{
			(var rooms, var path) = ReadEntity<Room.Initializer[]>(name);

			if (rooms == null || path == null)
				return;

			RoomConnections.Apply(rooms);

			WriteEntity(rooms, name, path);
		}
	}
}
