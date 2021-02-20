using R136.Interfaces;

namespace R136.Entities.Animates
{
	class Vacuum : Animate
	{
		public static Animate FromInitializer(Initializer initializer)
			=> new Vacuum(initializer.ID, initializer.StartRoom);

		private Vacuum(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (StatusManager != null)
			{
				StatusManager.DecreaseHealth(HealthImpact.Severe);
				StatusManager.CurrentRoom = RoomID.SnakeCave;
			}
		}
	}
}
