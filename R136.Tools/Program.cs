using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using R136.Entities;

namespace R136.Tools
{
	class Program
	{
		private static readonly JsonSerializerOptions _serializerOptions 
			= new JsonSerializerOptions() { ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };

		static void Main(string[] args)
		{
			Console.Write("Process animates base? [y/N]: ");
			if (Console.ReadLine().Trim().ToLower() == "y")
			{
				ProcessAnimates();
				Console.WriteLine();
			}

			Console.Write("Process rooms base? [y/N]: ");
			if (Console.ReadLine().Trim().ToLower() == "y")
			{
				ProcessRooms();
				Console.WriteLine();
			}

		}

		private static string GetOutputPath(string path)
		{
			string fileName = Path.GetFileNameWithoutExtension(path);

			if (fileName.Length > 4 && fileName.ToLower().EndsWith("base"))
				fileName = fileName[..^4] + ".json";
			else
				fileName += ".new.json";

			Console.Write($"Output base filename, or Enter for '{fileName}' (.json added if omitted): ");
			string userEntry = Console.ReadLine();

			if (!string.IsNullOrWhiteSpace(userEntry))
			{
				if (!userEntry.ToLower().EndsWith(".json"))
					userEntry += ".json";
				
				fileName = userEntry;
			}
							
			return Path.Combine(Path.GetDirectoryName(path), fileName);
		}

		private static void ProcessAnimates()
		{
			Animate.Initializer[] animates;

			Console.Write("Animates base JSON file path: ");
			string jsonFilePath = Console.ReadLine().Trim();
			try
			{
				string jsonString = File.ReadAllText(jsonFilePath, Encoding.UTF8);
				animates = JsonSerializer.Deserialize<Animate.Initializer[]>(jsonString);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Error while reading JSON file: {e}");
				return;
			}

			jsonFilePath = GetOutputPath(jsonFilePath);
			try
			{
				string jsonString = JsonSerializer.Serialize(animates, _serializerOptions);
				File.WriteAllText(jsonFilePath, jsonString, Encoding.UTF8);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Error while writing JSON file: {e}");
				return;
			}

			Console.WriteLine($"Animates JSON file written to: {jsonFilePath}");

		}

		private static void ProcessRooms()
		{
			Room.Initializer[] rooms;

			Console.Write("Rooms base JSON file path: ");
			string jsonFilePath = Console.ReadLine().Trim();
			try
			{
				string jsonString = File.ReadAllText(jsonFilePath, Encoding.UTF8);
				rooms = JsonSerializer.Deserialize<Room.Initializer[]>(jsonString);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Error while reading JSON file: {e}");
				return;
			}

			for (int i = 0; i < rooms.Length; i++)
			{
				rooms[i].Connections = new Dictionary<Direction, RoomID>(6)
				{
					[Direction.East] = (RoomID)(i + 1),
					[Direction.West] = (RoomID)(i - 1),
					[Direction.North] = (RoomID)(i - 5),
					[Direction.South] = (RoomID)(i + 5)
				};
			}

			// Seperate layers
			for (int i = 0; i < rooms.Length; i += 20)
			{
				for (int j = 0; j < 16; j += 5)
				{
					rooms[i + j + 4].Connections.Remove(Direction.East);
					rooms[i + j].Connections.Remove(Direction.West);
				}
				for (int j = 0; j < 5; j++)
				{
					rooms[i + j].Connections.Remove(Direction.North);
					rooms[i + j + 15].Connections.Remove(Direction.South);
				}
			}

			// Connect layers
			for (int i = 0; i < LevelConnections.Length; i++)
			{
				var connection = LevelConnections[i];
				rooms[(int)connection.From].Connections[connection.Direction] = connection.To;
			}

			// Blocked routes
			for (int i = 0; i < BlockedConnections.Length; i++)
			{
				var blockedConnection = BlockedConnections[i];
				rooms[(int)blockedConnection.Key].Connections.Remove(blockedConnection.Value);
			}

			// Mark dark rooms
			for (int i = 20; i < rooms.Length; i++)
			{
				rooms[i].IsDark = (i != (int)RoomID.TLCave && i != (int)RoomID.RadioactiveCave);
			}

			RoomID[] forest = new RoomID[] 
			{
				RoomID.Forest0, RoomID.Forest1, RoomID.Forest2, RoomID.Forest4,
				RoomID.Forest5, RoomID.Forest7,
				RoomID.Forest10, RoomID.Forest11,
				RoomID.Forest15, RoomID.Forest16
			};
			
			// Mark forest
			foreach (var room in forest)
			{
				rooms[(int)room].IsForest = true;
			}

			jsonFilePath = GetOutputPath(jsonFilePath);
			try
			{
				string jsonString = JsonSerializer.Serialize(rooms, _serializerOptions);
				File.WriteAllText(jsonFilePath, jsonString, Encoding.UTF8);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Error while writing JSON file: {e}");
				return;
			}

			Console.WriteLine($"Rooms base JSON file written to: {jsonFilePath}");
		}

		private record LevelConnection {
			public RoomID From { get; init; }
			public Direction Direction { get; init; }
			public RoomID To { get; init; }
		}

		private static readonly LevelConnection[] LevelConnections =
		{
			new LevelConnection() { From = RoomID.Cemetery, Direction = Direction.Down, To = RoomID.CactusCave },
			new LevelConnection() { From = RoomID.Ruin, Direction = Direction.Down, To = RoomID.BlackCave },
			new LevelConnection() { From = RoomID.BlackCave, Direction = Direction.Up, To = RoomID.Ruin },
			new LevelConnection() { From = RoomID.HieroglyphsCave, Direction = Direction.Down, To = RoomID.TalkingCave },
			new LevelConnection() { From = RoomID.StormCave, Direction = Direction.Down, To = RoomID.EyeCave },
			new LevelConnection() { From = RoomID.SpiralstaircaseCave1, Direction = Direction.Down, To = RoomID.SpiralstaircaseCave2 },
			new LevelConnection() { From = RoomID.EyeCave, Direction = Direction.Up, To = RoomID.StormCave },
			new LevelConnection() { From = RoomID.SpiralstaircaseCave2, Direction = Direction.Up, To = RoomID.SpiralstaircaseCave1 },
			new LevelConnection() { From = RoomID.SpiralstaircaseCave2, Direction = Direction.Down, To = RoomID.SpiralstaircaseCave3 },
			new LevelConnection() { From = RoomID.TalkingCave, Direction = Direction.Up, To = RoomID.HieroglyphsCave },
			new LevelConnection() { From = RoomID.SpiralstaircaseCave3, Direction = Direction.Up, To = RoomID.SpiralstaircaseCave2 }
		};

		private static readonly KeyValuePair<RoomID, Direction>[] BlockedConnections =
		{
			new KeyValuePair<RoomID, Direction>(RoomID.NorthSwamp, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.NorthSwamp, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.NorthSwamp, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.MiddleSwamp, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.MiddleSwamp, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.MiddleSwamp, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.MiddleSwamp, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.Forest5, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.Cemetery, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.Forest11, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.EmptySpace12, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.SouthSwamp, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.SouthSwamp, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.SouthSwamp, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.Ruin, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.SlimeCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.BlackCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.DrugCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.DrugCave, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.HornyCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.HornyCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.StraightjacketCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.NeglectedCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.NeglectedCave, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.EmptyCave26, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.EmptyCave26, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.MainCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.MainCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.MainCave, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.HieroglyphsCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.TLCave, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.SmallCave, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.IceCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.CactusCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.CactusCave, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.StormCave, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.MistCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.MistCave, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.MistCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.TentacleCave, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.GarbageCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.EchoCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.EchoCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.SecretCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.SecretCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.FoodCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.FoodCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.GnuCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.EmptyCave45, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.EyeCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.RockCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.Emptiness, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.SafeCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.SafeCave, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.NarrowCleft, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.NarrowCleft, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.NarrowCleft, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.OilCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.EmptyCave55, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.SpiralstaircaseCave2, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.SpiderCave, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.TalkingCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.LavaPit, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.ScoobyCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.RadioactiveCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.RadioactiveCave, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.DeathCave, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.RCave, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.RCave, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.ECave, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.DamnationCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.DamnationCave, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.VacuumCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.VacuumCave, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.VacuumCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.RedCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.RedCave, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.RedCave, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.NeonCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.BloodCave, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.BatCave, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.TeleportCave, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.TeleportCave, Direction.North)
		};
	}
}
