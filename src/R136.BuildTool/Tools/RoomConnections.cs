using R136.Entities;
using R136.Interfaces;
using System.Collections.Generic;

namespace R136.BuildTool.Tools
{
	class RoomConnections
	{
		public static void Apply(Room.Initializer[] rooms)
		{
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
					rooms[i + j + 4].Connections!.Remove(Direction.East);
					rooms[i + j].Connections!.Remove(Direction.West);
				}
				for (int j = 0; j < 5; j++)
				{
					rooms[i + j].Connections!.Remove(Direction.North);
					rooms[i + j + 15].Connections!.Remove(Direction.South);
				}
			}

			// Connect layers
			for (int i = 0; i < _levelConnections.Length; i++)
			{
				var connection = _levelConnections[i];
				rooms[(int)connection.From].Connections![connection.Direction] = connection.To;
			}

			// Blocked routes
			for (int i = 0; i < _blockedConnections.Length; i++)
			{
				var blockedConnection = _blockedConnections[i];
				rooms[(int)blockedConnection.Key].Connections!.Remove(blockedConnection.Value);
			}

			// Mark dark rooms
			for (int i = 20; i < rooms.Length; i++)
				rooms[i].IsDark = (i != (int)RoomID.FluorescentCave && i != (int)RoomID.RadioactiveCave);

			// Mark forest
			foreach (var room in _forestRoomIDs)
				rooms[(int)room].IsForest = true;
		}

		private record LevelConnection
		{
			public RoomID From { get; init; }
			public Direction Direction { get; init; }
			public RoomID To { get; init; }
		}

		private static readonly LevelConnection[] _levelConnections =
		{
			new() { From = RoomID.Cemetery, Direction = Direction.Down, To = RoomID.CactusCave },
			new() { From = RoomID.Ruin, Direction = Direction.Down, To = RoomID.BlackCave },
			new() { From = RoomID.BlackCave, Direction = Direction.Up, To = RoomID.Ruin },
			new() { From = RoomID.HieroglyphsCave, Direction = Direction.Down, To = RoomID.TalkingCave },
			new() { From = RoomID.StormCave, Direction = Direction.Down, To = RoomID.EyeCave },
			new() { From = RoomID.SpiralstaircaseCave1, Direction = Direction.Down, To = RoomID.SpiralstaircaseCave2 },
			new() { From = RoomID.EyeCave, Direction = Direction.Up, To = RoomID.StormCave },
			new() { From = RoomID.SpiralstaircaseCave2, Direction = Direction.Up, To = RoomID.SpiralstaircaseCave1 },
			new() { From = RoomID.SpiralstaircaseCave2, Direction = Direction.Down, To = RoomID.SpiralstaircaseCave3 },
			new() { From = RoomID.TalkingCave, Direction = Direction.Up, To = RoomID.HieroglyphsCave },
			new() { From = RoomID.SpiralstaircaseCave3, Direction = Direction.Up, To = RoomID.SpiralstaircaseCave2 }
		};

		private static readonly KeyValuePair<RoomID, Direction>[] _blockedConnections =
		{
			new(RoomID.NorthSwamp, Direction.West),
			new(RoomID.NorthSwamp, Direction.East),
			new(RoomID.NorthSwamp, Direction.South),
			new(RoomID.MiddleSwamp, Direction.West),
			new(RoomID.MiddleSwamp, Direction.East),
			new(RoomID.MiddleSwamp, Direction.North),
			new(RoomID.MiddleSwamp, Direction.South),
			new(RoomID.Forest5, Direction.East),
			new(RoomID.Cemetery, Direction.West),
			new(RoomID.Forest11, Direction.East),
			new(RoomID.EmptySpace12, Direction.West),
			new(RoomID.SouthSwamp, Direction.West),
			new(RoomID.SouthSwamp, Direction.East),
			new(RoomID.SouthSwamp, Direction.North),
			new(RoomID.Ruin, Direction.West),
			new(RoomID.SlimeCave, Direction.East),
			new(RoomID.BlackCave, Direction.West),
			new(RoomID.DrugCave, Direction.East),
			new(RoomID.DrugCave, Direction.South),
			new(RoomID.HornyCave, Direction.West),
			new(RoomID.HornyCave, Direction.East),
			new(RoomID.StraitjacketCave, Direction.West),
			new(RoomID.NeglectedCave, Direction.East),
			new(RoomID.NeglectedCave, Direction.North),
			new(RoomID.EmptyCave26, Direction.West),
			new(RoomID.EmptyCave26, Direction.East),
			new(RoomID.MainCave, Direction.East),
			new(RoomID.MainCave, Direction.West),
			new(RoomID.MainCave, Direction.North),
			new(RoomID.HieroglyphsCave, Direction.West),
			new(RoomID.FluorescentCave, Direction.South),
			new(RoomID.SmallCave, Direction.North),
			new(RoomID.IceCave, Direction.East),
			new(RoomID.CactusCave, Direction.West),
			new(RoomID.CactusCave, Direction.South),
			new(RoomID.StormCave, Direction.North),
			new(RoomID.MistCave, Direction.West),
			new(RoomID.MistCave, Direction.North),
			new(RoomID.MistCave, Direction.East),
			new(RoomID.TentacleCave, Direction.North),
			new(RoomID.GarbageCave, Direction.East),
			new(RoomID.EchoCave, Direction.West),
			new(RoomID.EchoCave, Direction.East),
			new(RoomID.SecretCave, Direction.West),
			new(RoomID.SecretCave, Direction.East),
			new(RoomID.FoodCave, Direction.West),
			new(RoomID.FoodCave, Direction.East),
			new(RoomID.GnuCave, Direction.West),
			new(RoomID.EmptyCave45, Direction.North),
			new(RoomID.EyeCave, Direction.East),
			new(RoomID.RockCave, Direction.West),
			new(RoomID.Emptiness, Direction.South),
			new(RoomID.SafeCave, Direction.East),
			new(RoomID.SafeCave, Direction.South),
			new(RoomID.NarrowCleft, Direction.North),
			new(RoomID.NarrowCleft, Direction.East),
			new(RoomID.NarrowCleft, Direction.South),
			new(RoomID.OilCave, Direction.West),
			new(RoomID.EmptyCave55, Direction.East),
			new(RoomID.SpiralstaircaseCave2, Direction.West),
			new(RoomID.SpiderCave, Direction.North),
			new(RoomID.TalkingCave, Direction.East),
			new(RoomID.LavaPit, Direction.West),
			new(RoomID.ScoobyCave, Direction.East),
			new(RoomID.RadioactiveCave, Direction.West),
			new(RoomID.RadioactiveCave, Direction.South),
			new(RoomID.DeathCave, Direction.South),
			new(RoomID.RCave, Direction.North),
			new(RoomID.RCave, Direction.South),
			new(RoomID.ECave, Direction.South),
			new(RoomID.DamnationCave, Direction.East),
			new(RoomID.DamnationCave, Direction.North),
			new(RoomID.VacuumCave, Direction.West),
			new(RoomID.VacuumCave, Direction.North),
			new(RoomID.VacuumCave, Direction.East),
			new(RoomID.RedCave, Direction.West),
			new(RoomID.RedCave, Direction.North),
			new(RoomID.RedCave, Direction.East),
			new(RoomID.NeonCave, Direction.West),
			new(RoomID.BloodCave, Direction.South),
			new(RoomID.BatCave, Direction.North),
			new(RoomID.TeleportCave, Direction.West),
			new(RoomID.TeleportCave, Direction.North)
		};

		private static readonly RoomID[] _forestRoomIDs = new RoomID[]
		{
			RoomID.Forest0, RoomID.Forest1, RoomID.Forest2, RoomID.Forest4,
			RoomID.Forest5, RoomID.Forest7,
			RoomID.Forest10, RoomID.Forest11,
			RoomID.Forest15, RoomID.Forest16
		};

	}
}
