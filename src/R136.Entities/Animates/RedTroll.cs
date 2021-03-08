using R136.Entities.Global;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	class RedTroll : Animate
	{
		public static Animate Create(Initializer initializer)
			=> new RedTroll(initializer.ID, initializer.StartRoom);

		private RedTroll(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (Facilities.Configuration.AutoPlaceItems)
				StatusManager?.Place(ItemID.RedCrystal);

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
					Status = AnimateStatus.PreparingNextAttack;

					break;

				case AnimateStatus.PreparingNextAttack:
					if (Facilities.Randomizer.Next(2) == 0)
						Status = AnimateStatus.Attack;

					break;

				case AnimateStatus.Dying:
					StatusManager?.Place(ItemID.RedCrystal);
					Status = AnimateStatus.Done;

					break;
			}
		}

		public override Result ApplyItem(ItemID item)
		{
			if (item != ItemID.Booklet)
				return Result.Error();

			Status = AnimateStatus.Dying;
			Trigger();
			return Result.Success();
		}
	}
}
