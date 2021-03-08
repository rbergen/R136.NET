using R136.Entities.Global;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	class Computer : Animate
	{
		public static Animate Create(Initializer initializer)
			=> new Computer(initializer.ID, initializer.StartRoom);

		private Computer(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			switch (status)
			{
				case AnimateStatus.Initial:
					if (!Facilities.Configuration.FreezeAnimates)
						Status = AnimateStatus.FirstWait;

					break;

				case AnimateStatus.Operating:
					Status = AnimateStatus.Done;

					break;
			}
		}

		public override Result ApplyItem(ItemID item)
		{
			if (item != ItemID.Diskette)
				return Result.Error();

			Status = AnimateStatus.Operating;
			Trigger();
			return Result.Success();
		}
	}
}
