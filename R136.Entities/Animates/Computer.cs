using R136.Entities.General;

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

		public override Result Used(ItemID item)
		{
			if (item != ItemID.Diskette)
				return Result.Failure();

			Status = AnimateStatus.Operating;
			return Result.Success();
		}
	}
}
