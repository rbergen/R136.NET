using System;

namespace R136.Entities
{
	public class RequestedRoomChange
	{
		public RoomID From { get; }
		public RoomID To { get; }

		public RequestedRoomChange(RoomID from, RoomID to)
			=> (From, To) = (from, to);
	}

	interface INotifyRoomChangeRequested
	{
		public Func<RequestedRoomChange, bool> RoomChangeRequestedHandler { get; }
	}
}
