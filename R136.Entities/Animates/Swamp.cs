namespace R136.Entities.Animates
{
	class Swamp : Animate
	{
		public Swamp(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (StatusManager == null)
				return;

			switch (ID)
			{
				case AnimateID.NorthSwamp:
					StatusManager.CurrentRoom = RoomID.EmptyCave51;

					break;

				case AnimateID.MiddleSwamp:
					StatusManager.CurrentRoom = RoomID.GloomyCave;

					break;

				case AnimateID.SouthSwamp:
					StatusManager.CurrentRoom = RoomID.RockCave;

					break;
			}
		}
	}
}
