using R136.Entities.Global;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	class HellHound : StrikableAnimate
	{
		public static Animate Create(Initializer initializer)
			=> new HellHound(initializer.ID, initializer.StartRoom, initializer.StrikeCount);

		private HellHound(AnimateID id, RoomID startRoom, int strikeCount) : base(id, startRoom, strikeCount) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (Facilities.Configuration.AutoPlaceItems)
				StatusManager?.Place(ItemID.HoundMeat);

			switch (status)
			{
				case AnimateStatus.Initial:
					if (!Facilities.Configuration.FreezeAnimates)
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
					StatusManager?.Place(ItemID.HoundMeat);
					Status = AnimateStatus.Done;

					break;
			}
		}
	}
}
