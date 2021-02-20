using R136.Interfaces;

namespace R136.Entities.General
{
	interface INotifyRoomChangeRequested
	{
		bool RoomChangeRequested(RoomID from, RoomID to);
		void RoomChanged(RoomID from, RoomID to);
	}
}
