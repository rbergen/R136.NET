using R136.Entities.General;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	class Barbecue : Animate
	{
		public Barbecue(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
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

		public override Result Used(ItemID item)
		{
			if (item != ItemID.Hashies && item != ItemID.HoundMeat)
				return Result.Error();

			Status = Status switch
			{
				AnimateStatus.Initial => item == ItemID.Hashies ? AnimateStatus.FirstStep : AnimateStatus.SecondStep,
				_ => AnimateStatus.Operating
			};

			return Result.Success();
		}
	}
}
