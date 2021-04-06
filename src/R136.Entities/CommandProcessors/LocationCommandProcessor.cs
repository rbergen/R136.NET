using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Collections.Generic;

namespace R136.Entities.CommandProcessors
{
	public class LocationCommandProcessor : CommandProcessor, IPaperRouteNotificationProvider, IRoomChangeNotificationProvider, IGameServiceProvider, ISnappable<LocationCommandProcessor.Snapshot>
	{
		public event Action? PaperRouteCompleted;
		public event Action<RoomChangeRequestedEventArgs>? RoomChanged;
		public event Action<RoomChangeRequestedEventArgs>? RoomChangeRequested;

		private int _paperrouteIndex = 0;

		public LocationCommandProcessor() : base(CommandProcessorID.Location) { }

		public override Result Execute(CommandID id, string command, string? parameters, Player player)
		{
			if (parameters != null)
				return Result.Error(GetTexts(TextID.CommandSyntax).ReplaceInAll("{command}", command));

			Direction? direction = CommandToDirection(id);

			if (direction == null)
				return Result.Error();

			if (!player.CurrentRoom.Connections.ContainsKey(direction.Value))
				return Result.Failure(GetTexts(TextID.CantGoThere));

			var fromRoom = player.CurrentRoom;
			var toRoom = player.CurrentRoom.Connections[direction.Value];

			var args = new RoomChangeRequestedEventArgs(fromRoom.ID, toRoom.ID);
			RoomChangeRequested?.Invoke(args);

			if (args.Cancel)
				return Result.Failure();

			player.CurrentRoom = toRoom;

			RoomChanged?.Invoke(args);
			CheckPaperRoute(toRoom.ID);

			return Result.Success();
		}

		private static Direction? CommandToDirection(CommandID id) 
			=> id switch
			{
				CommandID.GoEast => Direction.East,
				CommandID.GoWest => Direction.West,
				CommandID.GoNorth => Direction.North,
				CommandID.GoSouth => Direction.South,
				CommandID.GoUp => Direction.Up,
				CommandID.GoDown => Direction.Down,
				_ => null
			};

		private void CheckPaperRoute(RoomID toRoom)
		{
			var paperroute = Facilities.Configuration.PaperRoute;
			if (_paperrouteIndex < paperroute.Length)
			{
				if (paperroute[_paperrouteIndex] == toRoom)
					_paperrouteIndex++;
				else
					_paperrouteIndex = 0;

				if (_paperrouteIndex == paperroute.Length)
					PaperRouteCompleted?.Invoke();
			}
		}

		private StringValues GetTexts(TextID id)
			=> Facilities.TextsMap.Get(this, id);

		public Snapshot TakeSnapshot(Snapshot? snapshot = null)
		{
			if (snapshot == null)
				snapshot = new Snapshot();

			snapshot.PaperRouteIndex = _paperrouteIndex;

			return snapshot;
		}

		public bool RestoreSnapshot(Snapshot snapshot)
		{
			_paperrouteIndex = snapshot.PaperRouteIndex;

			return true;
		}

		public void RegisterServices(IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<IRoomChangeNotificationProvider>(this);
			serviceCollection.AddSingleton<IPaperRouteNotificationProvider>(this);
		}

		public class Snapshot : ISnapshot
		{
			public int ID { get; set; }
			public int PaperRouteIndex { get; set; }

			public void AddBytes(List<byte> bytes)
			{
				ID.AddBytes(bytes);
				PaperRouteIndex.AddBytes(bytes);
			}

			public int? LoadBytes(ReadOnlyMemory<byte> bytes)
			{
				int? bytesRead;
				int totalBytesRead = 0;

				(ID, bytesRead) = bytes.ToInt();
				if (bytesRead == null) return null;

				bytes = bytes[bytesRead.Value..];
				totalBytesRead += bytesRead.Value;

				(PaperRouteIndex, bytesRead) = bytes.ToInt();

				return bytesRead != null ? totalBytesRead + bytesRead.Value : null;
			}
		}

		private enum TextID : byte
		{
			CommandSyntax,
			CantGoThere
		}
	}
}
