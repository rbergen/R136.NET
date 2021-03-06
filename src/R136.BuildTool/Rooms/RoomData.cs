using R136.Entities;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.BuildTool.Rooms
{
	class RoomData
	{
		public Room.Initializer[]? Rooms { get; set; }
		public LevelConnection[]? LevelConnections { get; set; }
		public BlockedConnection[]? BlockedConnections { get; set; }
		public RoomID FirstCave { get; set; }
		public RoomID[]? LightCaves { get; set; }
		public RoomID[]? ForestRooms { get; set; }

		public Room.Initializer[]? Process(string? indent)
		{
			if (Rooms == null)
				return null;

			var rooms = new Room.Initializer[Rooms.Length];

			for (int i = 0; i < rooms.Length; i++)
			{
				rooms[i] = new()
				{
					ID = Rooms[i].ID,
					Name = Rooms[i].Name,
					Description = Rooms[i].Description
				};

				if (i >= (int)FirstCave)
					rooms[i].IsDark = !(LightCaves?.Contains((RoomID)i) ?? false);

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

			if (LevelConnections != null)
			{
				// Connect layers
				foreach (var connection in LevelConnections)
					rooms[(int)connection.From].Connections![connection.Direction] = connection.To;
			}

			if (BlockedConnections != null)
			{
				// Blocked routes
				foreach (var blockedConnection in BlockedConnections)
					rooms[(int)blockedConnection.Room].Connections!.Remove(blockedConnection.Direction);
			}

			// Mark dark rooms
			if (ForestRooms != null)
			{
				// Mark forest
				foreach (var room in ForestRooms)
					rooms[(int)room].IsForest = true;
			}

			return rooms;
		}
	}
}
