using R136.BuildTool.Tools;
using R136.Entities;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace R136.BuildTool.Rooms
{
	class RoomData
	{
		public Room.Initializer[]? Rooms { get; set; }
		public int LevelWidth { get; set; }
		public int LevelDepth { get; set; }
		public RoomID FirstCave { get; set; }
		public RoomID[]? LightCaves { get; set; }
		public LevelConnection[]? LevelConnections { get; set; }
		public BlockedConnection[]? BlockedConnections { get; set; }
		public RoomID[]? ForestRooms { get; set; }

		public Room.Initializer[]? CompileConnections(string? indent = null)
		{
			if (indent == null)
				indent = string.Empty;

			if (Rooms == null)
			{
				Console.WriteLine($"{indent}{Tags.Error} No room definitions found, aborting.");
				return null;
			}

			Console.WriteLine($"{indent}{Tags.Info} Found {Rooms.Length} room definitions.");

			Console.WriteLine($"{indent}{Tags.Info} Connecting rooms with direct neighbours, marking rooms from {FirstCave} onwards as dark with {LightCaves?.Length ?? 0} exceptions.");

			for (int i = 0; i < Rooms.Length; i++)
			{
				if (i >= (int)FirstCave)
					Rooms[i].IsDark = !(LightCaves?.Contains((RoomID)i) ?? false);

				Rooms[i].Connections = new Dictionary<Direction, RoomID>(6)
				{
					[Direction.East] = (RoomID)(i + 1),
					[Direction.West] = (RoomID)(i - 1),
					[Direction.North] = (RoomID)(i - LevelWidth),
					[Direction.South] = (RoomID)(i + LevelWidth)
				};
			}

			Console.WriteLine($"{indent}{Tags.Info} Separating rooms into levels of {LevelWidth} by {LevelDepth} rooms.");

			int levelSize = LevelWidth * LevelDepth;
			int levelDisconnectShift = levelSize - LevelWidth;

			// Seperate layers
			for (int i = 0; i < Rooms.Length; i += levelSize)
			{
				for (int j = 0; j < levelSize; j += LevelWidth)
				{
					Rooms[i + j + LevelWidth - 1].Connections!.Remove(Direction.East);
					Rooms[i + j].Connections!.Remove(Direction.West);
				}

				for (int j = 0; j < LevelWidth; j++)
				{
					Rooms[i + j].Connections!.Remove(Direction.North);
					Rooms[i + j + levelDisconnectShift].Connections!.Remove(Direction.South);
				}
			}

			if (LevelConnections != null)
			{
				Console.WriteLine($"{indent}{Tags.Info} Making {LevelConnections.Length} connections between levels.");

				// Connect layers
				foreach (var connection in LevelConnections)
					Rooms[(int)connection.From].Connections![connection.Direction] = connection.To;
			}

			if (BlockedConnections != null)
			{
				Console.WriteLine($"{indent}{Tags.Info} Removing {BlockedConnections.Length} connections between rooms.");

				// Blocked routes
				foreach (var blockedConnection in BlockedConnections)
					Rooms[(int)blockedConnection.Room].Connections!.Remove(blockedConnection.Direction);
			}

			return Rooms;
		}
	}
}
