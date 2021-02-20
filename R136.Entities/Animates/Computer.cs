using R136.Interfaces;

namespace R136.Entities.Animates
{
	class Computer : Animate
	{
		public static Animate FromInitializer(Initializer initializer)
			=> new Computer(initializer.ID, initializer.StartRoom);

		private Computer(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
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
				return Result.Error();

			Status = AnimateStatus.Operating;
			return Result.Success();
		}
	}
}
