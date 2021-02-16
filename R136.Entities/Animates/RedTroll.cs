using R136.Entities.Global;

namespace R136.Entities.Animates
{
	public class RedTroll : Animate
	{
		public RedTroll(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

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
					Status = AnimateStatus.PreparingNextAttack;

					break;

				case AnimateStatus.PreparingNextAttack:
					if (Facilities.Randomizer.Next(2) == 0)
						Status = AnimateStatus.Attack;

					break;

				case AnimateStatus.Dying:
					StatusManager?.PutDown(ItemID.RedCrystal);
					Status = AnimateStatus.Done;

					break;
			}
		}

		public override bool Used(ItemID item)
		{
			if (item != ItemID.Pornbook)
				return false;

			Status = AnimateStatus.Dying;
			return true;
		}
	}
}
