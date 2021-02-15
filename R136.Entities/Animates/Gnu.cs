using R136.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Animates
{
	public class Gnu : Animate
	{
		public static StatusTextMapper? StatusTexts { get; set; }

		public Gnu(AnimateID id, RoomID startRoom) : base(id, startRoom, StatusTexts) { }

		public override void ProcessStatusInternal(AnimateStatus status)
		{
			switch (status)
			{
				case AnimateStatus.Initial:
					Status = AnimateStatus.Attack;

					break;

				case AnimateStatus.Attack:
					StatusManager?.DecreaseHealth();

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
