using R136.Entities.Global;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	class Dragon : Animate
	{
		public static Animate Create(Initializer initializer)
			=> new Dragon(initializer.ID, initializer.StartRoom);

		private Dragon(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (Facilities.Configuration.AutoPlaceItems)
				StatusManager?.Place(ItemID.GasCanister);

			switch (status)
			{
				case AnimateStatus.Initial:
					if (!Facilities.Configuration.FreezeAnimates)
						Status = AnimateStatus.PreparingFirstAttack;

					break;

				case AnimateStatus.PreparingFirstAttack:
					Status = AnimateStatus.Attack;

					break;

				case AnimateStatus.Attack:
					Player?.DecreaseHealth();

					break;

				case AnimateStatus.FirstStep:
					Status = AnimateStatus.FirstWait;

					break;

				case AnimateStatus.SecondStep:
					StatusManager?.Place(ItemID.GasCanister);
					Status = AnimateStatus.Done;

					break;
			}
		}

		public override Result ApplyItem(ItemID item)
		{
			if (item != ItemID.Cookie && item != ItemID.Nightcap)
				return Result.Error();

			if (item == ItemID.Cookie)
			{
				Status = AnimateStatus.FirstStep;
				Trigger();
				return Result.Success();
			}

			if (item == ItemID.Nightcap && Status == AnimateStatus.FirstWait)
			{
				Status = AnimateStatus.SecondStep;
				Trigger();
				return Result.Success();
			}

			return Result.Failure();
		}
	}
}
