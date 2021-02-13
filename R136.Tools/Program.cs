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
					connectionsList[i + j + 15].Remove(Direction.North);
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
			new LevelConnection() { From = RoomID.Begraafplaats, Direction = Direction.Down, To = RoomID.KaktusGrot },
			new LevelConnection() { From = RoomID.Ruine, Direction = Direction.Down, To = RoomID.ZwarteGrot },
			new LevelConnection() { From = RoomID.ZwarteGrot, Direction = Direction.Up, To = RoomID.Ruine },
			new LevelConnection() { From = RoomID.HierogliefenGrot, Direction = Direction.Down, To = RoomID.PratendeGrot },
			new LevelConnection() { From = RoomID.StormGrot, Direction = Direction.Down, To = RoomID.OgenGrot },
			new LevelConnection() { From = RoomID.WenteltrapGrot1, Direction = Direction.Down, To = RoomID.WenteltrapGrot2 },
			new LevelConnection() { From = RoomID.OgenGrot, Direction = Direction.Up, To = RoomID.StormGrot },
			new LevelConnection() { From = RoomID.WenteltrapGrot2, Direction = Direction.Up, To = RoomID.WenteltrapGrot1 },
			new LevelConnection() { From = RoomID.WenteltrapGrot2, Direction = Direction.Down, To = RoomID.WenteltrapGrot3 },
			new LevelConnection() { From = RoomID.PratendeGrot, Direction = Direction.Up, To = RoomID.HierogliefenGrot },
			new LevelConnection() { From = RoomID.WenteltrapGrot3, Direction = Direction.Up, To = RoomID.WenteltrapGrot2 }
		};

		private static readonly KeyValuePair<RoomID, Direction>[] BlockedConnections =
		{
			new KeyValuePair<RoomID, Direction>(RoomID.NoordMoeras, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.NoordMoeras, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.NoordMoeras, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.MiddenMoeras, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.MiddenMoeras, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.MiddenMoeras, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.MiddenMoeras, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.Bos5, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.Begraafplaats, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.Bos11, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.OpenPlek12, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.ZuidMoeras, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.ZuidMoeras, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.ZuidMoeras, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.Ruine, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.SlijmGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.ZwarteGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.DrugsGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.DrugsGrot, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.GeileGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.GeileGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.DwangbuisGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.VerwaarloosdeGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.VerwaarloosdeGrot, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.LegeGrot26, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.LegeGrot26, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.HoofdGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.HoofdGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.HoofdGrot, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.HierogliefenGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.TLGrot, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.KleineGrot, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.IJsGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.KaktusGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.KaktusGrot, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.StormGrot, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.MistGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.MistGrot, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.MistGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.TentakelGrot, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.VuilnisGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.EchoGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.EchoGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.GeheimeGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.GeheimeGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.VoedselGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.VoedselGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.GnoeGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.LegeGrot45, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.OgenGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.RotsGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.Leegte, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.VeiligeGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.VeiligeGrot, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.NauweRotsspleet, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.NauweRotsspleet, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.NauweRotsspleet, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.OlieGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.LegeGrot55, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.WenteltrapGrot2, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.SpinnenGrot, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.PratendeGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.LavaPut, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.SkoebieGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.RadioactieveGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.RadioactieveGrot, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.DodenGrot, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.RGrot, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.RGrot, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.EGrot, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.VerdoemenisGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.VerdoemenisGrot, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.VacuumGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.VacuumGrot, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.VacuumGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.RodeGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.RodeGrot, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.RodeGrot, Direction.East),
			new KeyValuePair<RoomID, Direction>(RoomID.NeonGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.BloedGrot, Direction.South),
			new KeyValuePair<RoomID, Direction>(RoomID.VleermuisGrot, Direction.North),
			new KeyValuePair<RoomID, Direction>(RoomID.TeleportGrot, Direction.West),
			new KeyValuePair<RoomID, Direction>(RoomID.TeleportGrot, Direction.North)
		};
	}
}
