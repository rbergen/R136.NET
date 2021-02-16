using System;

namespace R136.Entities
{
	public class RoomChangeRequestedEventArgs : EventArgs
	{
		public RoomID From { get; }
		public RoomID To { get; }

		public RoomChangeRequestedEventArgs(RoomID from, RoomID to)
			=> (From, To) = (from, to);
	}

	public delegate bool RoomChangeRequestedHandler(object sender, RoomChangeRequestedEventArgs e);

	interface INotifyRoomChangeRequested
	{
		public RoomChangeRequestedHandler Handler { get; }
	}
}
