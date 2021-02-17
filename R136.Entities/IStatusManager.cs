namespace R136.Entities
{
	
	public interface IStatusManager
	{
		public void DecreaseHealth();
		public void DecreaseHealth(HealthImpact impact);
		public void RestoreHealth();
		public int LifePoints { get; }

		public RoomID CurrentRoom { get; set; }
		public bool IsDark { get; }

		public bool IsInPosession(ItemID item);
		public void RemoveFromPossession(ItemID item);

		public void PutDown(ItemID item);
		public void OpenConnection(Direction direction, RoomID toRoom);

		public bool RequestEnd { get; set; }
	}
}
