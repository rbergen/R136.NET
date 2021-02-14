using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Linq;

namespace R136.Entities
{
	public class Room
	{
		public string Name { get; private set; }

		public string Description { get; private set; }

		public bool IsDark { get; private set; }

		public bool IsForest { get; private set; }

		public Dictionary<Direction, Room> Connections { get; private set; }

		public static Dictionary<RoomID, Room> FromInitializers(ICollection<Initializer> initializers)
		{
			Dictionary<RoomID, Room> rooms = new Dictionary<RoomID, Room>(initializers.Count);

			foreach(var initializer in initializers)
			{
				rooms[initializer.ID] = new Room()
				{
					Name = initializer.Name,
					Description = initializer.Description,
					IsDark = initializer.IsDark,
					IsForest = initializer.IsForest
				};
			}

			foreach(var initializer in initializers)
			{
				rooms[initializer.ID].Connections = initializer.Connections.ToDictionary(pair => pair.Key, pair => rooms[pair.Value]);
			}

			return rooms;
		}

		public class Initializer
		{
			public RoomID ID { get; set; }
			public string Name { get; set; }
			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			public string Description { get; set; }
			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
			public bool IsDark { get; set; }
			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
			public bool IsForest { get; set; }
			public Dictionary<Direction, RoomID> Connections { get; set; }
		}

	}

	public enum RoomID
	{
		Forest0 = 0, Forest1, Forest2, NorthSwamp, Forest4 = 4,
		Forest5 = 5, Cemetery, Forest7, MiddleSwamp, EmptySpace9 = 9,
		Forest10 = 10, Forest11, EmptySpace12, SwampPath, EmptySpace14 = 14,
		Forest15 = 15, Forest16, EmptySpace17, SouthSwamp, Ruin = 19,

		SlimeCave = 20, BlackCave,	DrugCave, HornyCave, StraightjacketCave = 24,
		NeglectedCave = 25, EmptyCave26, MainCave, HieroglyphsCave, StenchCave = 29,
		GloomyCave = 30, TLCave, SmallCave, IceCave, CactusCave = 34,
		StalagmiteCave = 35, StormCave, MistCave, SpiralstaircaseCave1, TentacleCave = 39,

		GarbageCave = 40, EchoCave, SecretCave, FoodCave, GnuCave = 44,
		EmptyCave45 = 45, EyeCave, RockCave, Emptiness, Sandbank = 49,
		TortureCave = 50, EmptyCave51, SafeCave, NarrowCleft, OilCave = 54,
		EmptyCave55 = 55,	SpiralstaircaseCave2, SpiderCave, TalkingCave, LavaPit = 59,
		
		ScoobyCave = 60, RadioactiveCave, ICave, PCave, ACave = 64,
		DeathCave = 65, RCave, ECave, SpiralstaircaseCave3, CapitalPCave = 69,
		DamnationCave = 70, VacuumCave, RedCave, NeonCave, BloodCave = 74,
		BatCave = 75, SnakeCave, LobeCave, SlipperyCave, TeleportCave = 79,

		None = 80
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

}
