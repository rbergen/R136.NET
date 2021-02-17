using R136.Entities.General;

namespace R136.Entities.Animates
{
	class Dragon : Animate
	{
		public Dragon(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			switch (status)
			{
				case AnimateStatus.Initial:
					Status = AnimateStatus.PreparingFirstAttack;

					break;

				case AnimateStatus.PreparingFirstAttack:
					Status = AnimateStatus.Attack;

					break;

				case AnimateStatus.Attack:
					StatusManager?.DecreaseHealth();

					break;

				case AnimateStatus.FirstStep:
					Status = AnimateStatus.FirstWait;

					break;

				case AnimateStatus.SecondStep:
					StatusManager?.PutDown(ItemID.GasCanister);
					Status = AnimateStatus.Done;

					break;
			}
		}

		public override Result Used(ItemID item)
		{
			if (item != ItemID.Cookie && item != ItemID.Nightcap)
				return Result.Error();

			if (item == ItemID.Cookie)
			{
				Status = AnimateStatus.FirstStep;
				return Result.Success();
			}

			if (item == ItemID.Nightcap && Status == AnimateStatus.FirstWait)
			{
				Status = AnimateStatus.SecondStep;
				return Result.Success();
			}

			return Result.Failure();
		}
	}
}
