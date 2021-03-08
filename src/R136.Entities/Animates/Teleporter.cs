using R136.Interfaces;

namespace R136.Entities.Animates
{
	class Teleporter : Animate
	{
		public static Animate Create(Initializer initializer)
			=> new Teleporter(initializer.ID, initializer.StartRoom);

		private Teleporter(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (Player != null)
				Player.CurrentRoom = RoomID.Forest1;
		}
	}
}
