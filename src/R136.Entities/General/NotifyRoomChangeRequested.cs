using R136.Interfaces;
using System.ComponentModel;

namespace R136.Entities.General
{
	interface INotifyRoomChangeRequested
	{
		void RoomChangeRequested(RoomChangeRequestedEventArgs args);
		void RoomChanged(RoomChangeRequestedEventArgs args);

	}

	public class RoomChangeRequestedEventArgs : CancelEventArgs
	{
		public RoomID From { get; init; }
		public RoomID To { get; init; }

		public RoomChangeRequestedEventArgs() { }

		public RoomChangeRequestedEventArgs(RoomID from, RoomID to)
			=> (From, To) = (from, to);
	}
}
