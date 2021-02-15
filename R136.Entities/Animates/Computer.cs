using R136.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Animates
{
	public class Computer : Animate
	{
		public static StatusTextMapper? StatusTexts { get; set; }

		public Computer(AnimateID id, RoomID startRoom) : base(id, startRoom, StatusTexts) { }

		public override void ProcessStatusInternal(AnimateStatus status)
		{
			switch (status)
			{
				case AnimateStatus.Initial:
					Status = AnimateStatus.FirstWait;

					break;

				case AnimateStatus.Operating:
					Status = AnimateStatus.Done;

					break;
			}
		}

		public override bool Used(ItemID item)
		{
			if (item != ItemID.Diskette)
				return false;

			Status = AnimateStatus.Operating;
			return true;
		}
	}
}
