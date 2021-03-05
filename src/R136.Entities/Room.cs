using R136.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace R136.Entities
{
	public class Room
	{
		public RoomID ID { get; }
		public string Name { get; }
		public string? Description { get; }
		public bool IsDark { get; }
		public bool IsForest { get; }

		public IDictionary<Direction, Room> Connections { get; private set; } = null!;

		public static IReadOnlyDictionary<RoomID, Room> CreateMap(ICollection<Initializer> initializers)
		{
			Dictionary<RoomID, Room> rooms = new(initializers.Count);

			foreach (var initializer in initializers)
			{
				rooms[initializer.ID] = new
					(
						initializer.ID,
						initializer.Name,
						initializer.Description,
						initializer.IsDark,
						initializer.IsForest
					);
			}

			foreach (var initializer in initializers)
				rooms[initializer.ID].Connections = initializer.Connections?.ToDictionary(pair => pair.Key, pair => rooms[pair.Value]) ?? new Dictionary<Direction, Room>();

			return rooms;
		}

		private Room(RoomID id, string name, string? description, bool isDark, bool isForest)
			=> (ID, Name, Description, IsDark, IsForest) = (id, name, description, isDark, isForest);

		public class Initializer
		{
			public RoomID ID { get; set; }
			public string Name { get; set; } = "";

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			public string? Description { get; set; }

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
			public bool IsDark { get; set; }

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
			public bool IsForest { get; set; }
			public Dictionary<Direction, RoomID>? Connections { get; set; }
		}
	}
}
