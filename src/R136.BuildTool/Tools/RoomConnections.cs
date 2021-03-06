using R136.BuildTool.Rooms;
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
				rooms[(int)blockedConnection.Room].Connections!.Remove(blockedConnection.Direction);
			}

			// Mark dark rooms
			for (int i = 20; i < rooms.Length; i++)
				rooms[i].IsDark = (i != (int)RoomID.FluorescentCave && i != (int)RoomID.RadioactiveCave);

			// Mark forest
			foreach (var room in _forestRoomIDs)
				rooms[(int)room].IsForest = true;
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

		private static readonly BlockedConnection[] _blockedConnections =
		{
			new() { Room = RoomID.NorthSwamp, Direction = Direction.West },
			new() { Room = RoomID.NorthSwamp, Direction = Direction.East },
			new() { Room = RoomID.NorthSwamp, Direction = Direction.South },
			new() { Room = RoomID.MiddleSwamp, Direction = Direction.West },
			new() { Room = RoomID.MiddleSwamp, Direction = Direction.East },
			new() { Room = RoomID.MiddleSwamp, Direction = Direction.North },
			new() { Room = RoomID.MiddleSwamp, Direction = Direction.South },
			new() { Room = RoomID.Forest5, Direction = Direction.East },
			new() { Room = RoomID.Cemetery, Direction = Direction.West },
			new() { Room = RoomID.Forest11, Direction = Direction.East },
			new() { Room = RoomID.EmptySpace12, Direction = Direction.West },
			new() { Room = RoomID.SouthSwamp, Direction = Direction.West },
			new() { Room = RoomID.SouthSwamp, Direction = Direction.East },
			new() { Room = RoomID.SouthSwamp, Direction = Direction.North },
			new() { Room = RoomID.Ruin, Direction = Direction.West },
			new() { Room = RoomID.SlimeCave, Direction = Direction.East },
			new() { Room = RoomID.BlackCave, Direction = Direction.West },
			new() { Room = RoomID.DrugCave, Direction = Direction.East },
			new() { Room = RoomID.DrugCave, Direction = Direction.South },
			new() { Room = RoomID.HornyCave, Direction = Direction.West },
			new() { Room = RoomID.HornyCave, Direction = Direction.East },
			new() { Room = RoomID.StraitjacketCave, Direction = Direction.West },
			new() { Room = RoomID.NeglectedCave, Direction = Direction.East },
			new() { Room = RoomID.NeglectedCave, Direction = Direction.North },
			new() { Room = RoomID.EmptyCave26, Direction = Direction.West },
			new() { Room = RoomID.EmptyCave26, Direction = Direction.East },
			new() { Room = RoomID.MainCave, Direction = Direction.East },
			new() { Room = RoomID.MainCave, Direction = Direction.West },
			new() { Room = RoomID.MainCave, Direction = Direction.North },
			new() { Room = RoomID.HieroglyphsCave, Direction = Direction.West },
			new() { Room = RoomID.FluorescentCave, Direction = Direction.South },
			new() { Room = RoomID.SmallCave, Direction = Direction.North },
			new() { Room = RoomID.IceCave, Direction = Direction.East },
			new() { Room = RoomID.CactusCave, Direction = Direction.West },
			new() { Room = RoomID.CactusCave, Direction = Direction.South },
			new() { Room = RoomID.StormCave, Direction = Direction.North },
			new() { Room = RoomID.MistCave, Direction = Direction.West },
			new() { Room = RoomID.MistCave, Direction = Direction.North },
			new() { Room = RoomID.MistCave, Direction = Direction.East },
			new() { Room = RoomID.TentacleCave, Direction = Direction.North },
			new() { Room = RoomID.GarbageCave, Direction = Direction.East },
			new() { Room = RoomID.EchoCave, Direction = Direction.West },
			new() { Room = RoomID.EchoCave, Direction = Direction.East },
			new() { Room = RoomID.SecretCave, Direction = Direction.West },
			new() { Room = RoomID.SecretCave, Direction = Direction.East },
			new() { Room = RoomID.FoodCave, Direction = Direction.West },
			new() { Room = RoomID.FoodCave, Direction = Direction.East },
			new() { Room = RoomID.GnuCave, Direction = Direction.West },
			new() { Room = RoomID.EmptyCave45, Direction = Direction.North },
			new() { Room = RoomID.EyeCave, Direction = Direction.East },
			new() { Room = RoomID.RockCave, Direction = Direction.West },
			new() { Room = RoomID.Emptiness, Direction = Direction.South },
			new() { Room = RoomID.SafeCave, Direction = Direction.East },
			new() { Room = RoomID.SafeCave, Direction = Direction.South },
			new() { Room = RoomID.NarrowCleft, Direction = Direction.North },
			new() { Room = RoomID.NarrowCleft, Direction = Direction.East },
			new() { Room = RoomID.NarrowCleft, Direction = Direction.South },
			new() { Room = RoomID.OilCave, Direction = Direction.West },
			new() { Room = RoomID.EmptyCave55, Direction = Direction.East },
			new() { Room = RoomID.SpiralstaircaseCave2, Direction = Direction.West },
			new() { Room = RoomID.SpiderCave, Direction = Direction.North },
			new() { Room = RoomID.TalkingCave, Direction = Direction.East },
			new() { Room = RoomID.LavaPit, Direction = Direction.West },
			new() { Room = RoomID.ScoobyCave, Direction = Direction.East },
			new() { Room = RoomID.RadioactiveCave, Direction = Direction.West },
			new() { Room = RoomID.RadioactiveCave, Direction = Direction.South },
			new() { Room = RoomID.DeathCave, Direction = Direction.South },
			new() { Room = RoomID.RCave, Direction = Direction.North },
			new() { Room = RoomID.RCave, Direction = Direction.South },
			new() { Room = RoomID.ECave, Direction = Direction.South },
			new() { Room = RoomID.DamnationCave, Direction = Direction.East },
			new() { Room = RoomID.DamnationCave, Direction = Direction.North },
			new() { Room = RoomID.VacuumCave, Direction = Direction.West },
			new() { Room = RoomID.VacuumCave, Direction = Direction.North },
			new() { Room = RoomID.VacuumCave, Direction = Direction.East },
			new() { Room = RoomID.RedCave, Direction = Direction.West },
			new() { Room = RoomID.RedCave, Direction = Direction.North },
			new() { Room = RoomID.RedCave, Direction = Direction.East },
			new() { Room = RoomID.NeonCave, Direction = Direction.West },
			new() { Room = RoomID.BloodCave, Direction = Direction.South },
			new() { Room = RoomID.BatCave, Direction = Direction.North },
			new() { Room = RoomID.TeleportCave, Direction = Direction.West },
			new() { Room = RoomID.TeleportCave, Direction = Direction.North }
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
