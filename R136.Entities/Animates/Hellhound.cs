using R136.Entities.Global;
using R136.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Animates
{
	public class HellHound : StrikableAnimate
	{

		public static StatusTextMapper? StatusTexts { get; set; }

		public HellHound(AnimateID id, RoomID startRoom, int strikeCount) : base(id, startRoom, strikeCount, StatusTexts) { }

		public override void ProcessStatusInternal(AnimateStatus status)
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
