using R136.Entities.General;
using R136.Entities.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.CommandProcessors
{
	class LocationCommandProcessor : CommandProcessor
	{
		private readonly IReadOnlyCollection<INotifyRoomChangeRequested> _notifiees;
		private readonly ITriggerable? _paperrouteTriggerable;
		private int _paperrouteIndex = 0;

		public LocationCommandProcessor(IReadOnlyCollection<INotifyRoomChangeRequested> notifiees, ITriggerable? paperrouteTriggerable)
			=> (_notifiees, _paperrouteTriggerable) = (notifiees, paperrouteTriggerable);

		public override Result Execute(CommandID id, string name, string? parameters, Player player, ICollection<Item> presentItems, Animate? presentAnimate)
		{
			if (parameters != null)
				Result.Error(GetTexts(TextID.CommandSyntax)?.ReplaceInAll("{command}", name));

			Direction? direction = CommandToDirection(id);

			if (direction == null)
				return Result.Error();

			if (!player.CurrentRoom.Connections.ContainsKey(direction.Value))
				return Result.Failure(GetTexts(TextID.CantGoThere));

			var toRoom = player.CurrentRoom.Connections[direction.Value];

			var proceed = true;

			foreach (var requestNotifiee in _notifiees)
				proceed &= requestNotifiee.RoomChangeRequested(player.CurrentRoom.ID, toRoom.ID);

			if (!proceed)
				return Result.Failure();

			player.CurrentRoom = toRoom;

			CheckPaperRoute(toRoom.ID);

			return Result.Success();
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

				if (_paperrouteIndex == paperroute.Length && _paperrouteTriggerable != null)
					_paperrouteTriggerable.Trigger();
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
