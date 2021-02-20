﻿using R136.Entities.Global;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	class RedTroll : Animate
	{
		public static Animate FromInitializer(Initializer initializer)
			=> new RedTroll(initializer.ID, initializer.StartRoom);

		private RedTroll(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

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

		public override Result Used(ItemID item)
		{
			if (item != ItemID.Pornbook)
				return Result.Error();

			Status = AnimateStatus.Dying;
			return Result.Success();
		}
	}
}
