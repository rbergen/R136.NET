using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.CommandProcessors
{
	class LocationCommandProcessor : ActingCommandProcessor
	{
		public event Action? PaperRouteCompleted;
		public event Action<RoomID, RoomID>? RoomChanged;

		private int _paperrouteIndex = 0;

		public LocationCommandProcessor(IReadOnlyDictionary<ItemID, Item> items, IReadOnlyDictionary<AnimateID, Animate> animates) : base(items, animates) 
		{
			RegisterRoomChangedListeners(items);
			RegisterRoomChangedListeners(animates);
		}

		public override Result Execute(CommandID id, string command, string? parameters, Player player)
		{
			if (parameters != null)
				Result.Error(GetTexts(TextID.CommandSyntax)?.ReplaceInAll("{command}", command));

			Direction? direction = CommandToDirection(id);

			if (direction == null)
				return Result.Error();

			if (!player.CurrentRoom.Connections.ContainsKey(direction.Value))
				return Result.Failure(GetTexts(TextID.CantGoThere));

			var fromRoom = player.CurrentRoom;
			var toRoom = player.CurrentRoom.Connections[direction.Value];

			if (!NotifyRoomChangeRequested(Items, fromRoom.ID, toRoom.ID) || !NotifyRoomChangeRequested(Animates, fromRoom.ID, toRoom.ID))
				return Result.Failure();

			player.CurrentRoom = toRoom;

			RoomChanged?.Invoke(fromRoom.ID, toRoom.ID);

			CheckPaperRoute(toRoom.ID);

			return Result.Success();
		}

		private static bool NotifyRoomChangeRequested<TEntityKey, TEntityValue>(IReadOnlyDictionary<TEntityKey, TEntityValue> entities, RoomID fromRoom, RoomID toRoom)
		{
			foreach (var requestNotifiee in entities.Values.Where(entity => entity is INotifyRoomChangeRequested).Cast<INotifyRoomChangeRequested>())
			{
				if (!requestNotifiee.RoomChangeRequested(fromRoom, toRoom))
					return false;
			}
			
			return true;
		}

		private void RegisterRoomChangedListeners<TEntityKey, TEntityValue>(IReadOnlyDictionary<TEntityKey, TEntityValue> entities)
		{
			foreach (var requestNotifiee in entities.Values.Where(entity => entity is INotifyRoomChangeRequested).Cast<INotifyRoomChangeRequested>())
				RoomChanged += requestNotifiee.RoomChanged;
		}

		private static Direction? CommandToDirection(CommandID id) => id switch
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
			if (_paperrouteIndex != paperroute.Length)
			{
				if (paperroute[_paperrouteIndex] == toRoom)
					_paperrouteIndex++;
				else
					_paperrouteIndex = 0;

				if (_paperrouteIndex == paperroute.Length)
					PaperRouteCompleted?.Invoke();
			}
		}

		private ICollection<string>? GetTexts(TextID id)
			=> Facilities.TextsMap[this, (int)id];

		private enum TextID
		{
			CommandSyntax,
			CantGoThere
		}
	}
}
