namespace R136.Interfaces
{
	public interface IPlayer
	{
		void DecreaseHealth();
		void DecreaseHealth(HealthImpact impact);
		void RestoreHealth();
		int LifePoints { get; }

		RoomID CurrentRoom { get; set; }
		bool IsDark { get; }

		bool IsInPosession(ItemID item);
		void RemoveFromPossession(ItemID item);

	}
}
