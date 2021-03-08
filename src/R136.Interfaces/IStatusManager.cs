namespace R136.Interfaces
{
	public interface IStatusManager
	{
		void Place(ItemID item);
		void OpenConnection(Direction direction, RoomID toRoom);
	}
}
