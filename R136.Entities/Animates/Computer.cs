namespace R136.Entities.Animates
{
	public class Computer : Animate
	{
		public Computer(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public override void ProgressStatusInternal(AnimateStatus status)
		{
			switch (status)
			{
				case AnimateStatus.Initial:
					Status = AnimateStatus.FirstWait;

					break;

				case AnimateStatus.Operating:
					Status = AnimateStatus.Done;

					break;
			}
		}

		public override bool Used(ItemID item)
		{
			if (item != ItemID.Diskette)
				return false;

			Status = AnimateStatus.Operating;
			return true;
		}
	}
}
