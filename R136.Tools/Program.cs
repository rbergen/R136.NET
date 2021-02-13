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
		static void Main(string[] args)
		{
			RoomInitializer[] rooms;

			Console.Write("Enter JSON file path: ");
			string jsonFilePath = Console.ReadLine().Trim();
			try
			{
				string jsonString = File.ReadAllText(jsonFilePath, Encoding.UTF8);
				rooms = JsonSerializer.Deserialize<RoomInitializer[]>(jsonString);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine($"Error while reading and parsing JSON file: {e}");
				return;
			}

			// We're using int instead of RoomID because we'll have some out-of-bound RoomIDs at the beginning
			var connectionsList = new Dictionary<Direction, int>[rooms.Length];

			for (int i = 0; i < rooms.Length; i++)
			{
				connectionsList[i] = new Dictionary<Direction, int>(4)
				{
					[Direction.East] = i + 1,
					[Direction.West] = i - 1,
					[Direction.North] = i - 5,
					[Direction.South] = i + 5
				};
			}

			// Seperate layers
			for (int i = 0; i < rooms.Length; i += 20)
			{
				for (int j = 0; j < 16; j += 5)
				{
					connectionsList[i + j + 4].Remove(Direction.East);
					connectionsList[i + j].Remove(Direction.West);
				}
				for (int j = 0; j < 5; j++)
				{
					connectionsList[i + j].Remove(Direction.North);
					connectionsList[i + j + 15].Remove(Direction.South);
				}
			}

			for (int i = 0; i < rooms.Length; i++)
			{
				rooms[i].Connections = connectionsList[i].ToDictionary(pair => pair.Key, pair => (RoomID)pair.Value);
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

			for (int i = 20; i < rooms.Length; i++)
			{
				rooms[i].IsDark = (i != (int)RoomID.TLCave && i != (int)RoomID.RadioactiveCave);
			}

			jsonFilePath += ".new";
			Console.WriteLine($"Output filename, or Enter for {Path.GetFileName(jsonFilePath)}: ");
			string jsonFileName = Console.ReadLine();
			if (!string.IsNullOrEmpty(jsonFileName))
				jsonFilePath = Path.Combine(Path.GetDirectoryName(jsonFilePath), jsonFileName);

			try
			{
				string jsonString = JsonSerializer.Serialize<RoomInitializer[]>(rooms, new JsonSerializerOptions() { WriteIndented = true });
				File.WriteAllText(jsonFilePath, jsonString, Encoding.UTF8);
			}
			catch(Exception e)
			{
				Console.Error.WriteLine($"Error while writing JSON file: {e}");
				return;
			}

			Console.WriteLine($"JSON file written to: {jsonFilePath}");
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
