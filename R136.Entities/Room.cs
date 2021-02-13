using System;
using System.Collections.Generic;
using System.Linq;

namespace R136.Entities
{
	public class Room
	{
		public string Name { get; private set; }

		public string Description { get; private set; }

		public bool IsDark { get; private set; }

		public Dictionary<Direction, Room> Connections { get; private set; }

		public static Dictionary<RoomID, Room> FromInitializers(RoomInitializer[] roomInitializers)
		{
			Dictionary<RoomID, Room> rooms = new Dictionary<RoomID, Room>(roomInitializers.Length);

			foreach(var initializer in roomInitializers)
			{
				rooms[initializer.ID] = new Room()
				{
					Name = initializer.Name,
					Description = initializer.Description,
					IsDark = initializer.IsDark
				};
			}

			foreach(var initializer in roomInitializers)
			{
				rooms[initializer.ID].Connections = initializer.Connections.ToDictionary(pair => pair.Key, pair => rooms[pair.Value]);
			}

			return rooms;
		}
	}

	public enum RoomID
	{
		Forest0, Forest1, Forest2, NorthSwamp, Forest4,
		Forest5, Cemetery, Forest7, MiddleSwamp, EmptySpace9,
		Forest10, Forest11, EmptySpace12, SwampPath, EmptySpace14,
		Forest15, Forest16, EmptySpace17, SouthSwamp, Ruin,

		SlimeCave, BlackCave,	DrugCave, HornyCave, StraightjacketCave,
		NeglectedCave, EmptyCave26, MainCave, HieroglyphsCave, StenchCave,
		GloomyCave, TLCave, SmallCave, IceCave, CactusCave,
		StalagmiteCave, StormCave, MistCave, SpiralstaircaseCave1, TentacleCave,

		GarbageCave, EchoCave, SecretCave, FoodCave, GnuCave,
		EmptyCave45, EyeCave, RockCave, Emptiness, Sandbank,
		TortureCave, EmptyCave51, SafeCave, NarrowCleft, OilCave,
		EmptyCave55,	SpiralstaircaseCave2, SpiderCave, TalkingCave, LavaPit,
		
		ScoobyCave, RadioactiveCave, ICave, PCave, ACave,
		DeathCave, RCave, ECave, SpiralstaircaseCave3, CapitalPCave,
		DamnationCave, VacuumCave, RedCave, NeonCave, BloodCave,
		BatCave, SnakeCave, LobeCave, SlipperyCave, TeleportCave
	}

	public enum Direction
	{
		East,
		West,
		North,
		South,
		Up,
		Down
	}

	public class RoomInitializer
	{
		public RoomID ID { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public bool IsDark { get; set; }
		public Dictionary<Direction, RoomID> Connections { get; set; }
	}
}
