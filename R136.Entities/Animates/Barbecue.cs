namespace R136.Entities.Animates
{
	public class Barbecue : Animate
	{
		public Barbecue(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public override void ProgressStatusInternal(AnimateStatus status)
		{
			switch (status)
			{
				case AnimateStatus.FirstStep:
					Status = AnimateStatus.FirstWait;

					break;

				case AnimateStatus.SecondStep:
					Status = AnimateStatus.FirstWait;

					break;

				case AnimateStatus.Operating:
					StatusManager?.PutDown(ItemID.Cookie);
					Status = AnimateStatus.Initial;

					break;
			}
		}

		public override bool Used(ItemID item)
		{
			if (item != ItemID.Hashies && item != ItemID.HoundMeat)
				return false;

			Status = (Status == AnimateStatus.Initial)
								? (item == ItemID.Hashies ? AnimateStatus.FirstStep : AnimateStatus.SecondStep)
								: AnimateStatus.Operating;

			return true;
		}
	}
}
