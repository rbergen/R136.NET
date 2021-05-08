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

		private int paperrouteIndex = 0;

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

			Room fromRoom = player.CurrentRoom;
			Room toRoom = player.CurrentRoom.Connections[direction.Value];

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
			if (this.paperrouteIndex < paperroute.Length)
			{
				if (paperroute[this.paperrouteIndex] == toRoom)
					this.paperrouteIndex++;
				else
					this.paperrouteIndex = 0;

				if (this.paperrouteIndex == paperroute.Length)
					PaperRouteCompleted?.Invoke();
			}
		}

		private StringValues GetTexts(TextID id)
			=> Facilities.TextsMap.Get(this, id);

		public Snapshot TakeSnapshot(Snapshot? snapshot = null)
		{
			if (snapshot == null)
				snapshot = new Snapshot();

			snapshot.PaperRouteIndex = this.paperrouteIndex;

			return snapshot;
		}

		public bool RestoreSnapshot(Snapshot snapshot)
		{
			this.paperrouteIndex = snapshot.PaperRouteIndex;

			return true;
		}

		public void RegisterServices(IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<IRoomChangeNotificationProvider>(this);
			serviceCollection.AddSingleton<IPaperRouteNotificationProvider>(this);
		}

		public class Snapshot : ISnapshot
		{
			public int PaperRouteIndex { get; set; }

			public void AddBytes(List<byte> bytes)
				=> PaperRouteIndex.AddBytes(bytes);

			public int? LoadBytes(ReadOnlyMemory<byte> bytes)
			{
				int? bytesRead;

				(PaperRouteIndex, bytesRead) = bytes.ToInt();

				return bytesRead;
			}
		}

		private enum TextID : byte
		{
			CommandSyntax,
			CantGoThere
		}
	}
}
