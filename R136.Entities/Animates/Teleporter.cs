using R136.Interfaces;

namespace R136.Entities.Animates
{
	class Teleporter : Animate
	{
		public static Animate FromInitializer(Initializer initializer)
			=> new Teleporter(initializer.ID, initializer.StartRoom);

		private Teleporter(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (StatusManager != null)
			{
				StatusManager.CurrentRoom = RoomID.Forest1;
			}
		}
	}
}
