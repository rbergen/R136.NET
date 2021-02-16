namespace R136.Entities.Animates
{
	public class Dragon : Animate
	{
		public Dragon(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public override void ProgressStatusInternal(AnimateStatus status)
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

		public override bool Used(ItemID item)
		{
			if (item == ItemID.Cookie)
			{
				Status = AnimateStatus.FirstStep;
				return true;
			}

			if (item == ItemID.Nightcap && Status == AnimateStatus.FirstWait)
			{
				Status = AnimateStatus.SecondStep;
				return true;
			}

			return false;
		}
	}
}
