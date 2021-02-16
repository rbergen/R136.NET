using R136.Entities.Global;

namespace R136.Entities.Animates
{
	public class HellHound : StrikableAnimate
	{

		public HellHound(AnimateID id, RoomID startRoom, int strikeCount) : base(id, startRoom, strikeCount) { }

		public override void ProgressStatusInternal(AnimateStatus status)
		{
			switch (status)
			{
				case AnimateStatus.Initial:
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
					StatusManager?.PutDown(ItemID.HoundMeat);
					Status = AnimateStatus.Done;

					break;
			}
		}
	}
}
