using R136.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Animates
{
	public class Barbecue : Animate
	{
		public static StatusTextMapper? StatusTexts { get; set; }

		public Barbecue(IServiceProvider serviceProvider, RoomID startRoom) : base(serviceProvider, startRoom, StatusTexts) { }

		public override void ProcessStatusInternal(AnimateStatus status)
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
					StatusManager?.ReleaseItem(ItemID.Cookie);
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
