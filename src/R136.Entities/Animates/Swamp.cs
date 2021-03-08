using R136.Interfaces;

namespace R136.Entities.Animates
{
	class Swamp : Animate
	{
		public static Animate Create(Initializer initializer)
			=> new Swamp(initializer.ID, initializer.StartRoom);

		private Swamp(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (Player == null)
				return;

			Trigger();

			switch (ID)
			{
				case AnimateID.NorthSwamp:
					Player.CurrentRoom = RoomID.EmptyCave51;

					break;

				case AnimateID.MiddleSwamp:
					Player.CurrentRoom = RoomID.GloomyCave;

					break;

				case AnimateID.SouthSwamp:
					Player.CurrentRoom = RoomID.RockCave;

					break;
			}
		}
	}
}
