using R136.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Animates
{
	public class DragonHead : Animate
	{
		public static StatusTextMapper? StatusTexts { get; set; }

		public DragonHead(AnimateID id, RoomID startRoom) : base(id, startRoom, StatusTexts) { }

		public override void ProcessStatusInternal(AnimateStatus status)
		{
			switch (status)
			{
				case AnimateStatus.FirstStep:
					Status = AnimateStatus.FirstWait;

					break;

				case AnimateStatus.SecondStep:
					Status = AnimateStatus.SecondWait;

					break;

				case AnimateStatus.Operating:
					StatusManager?.OpenConnection(Direction.North, RoomID.MainCave);
					Status = AnimateStatus.Done;

					break;
			}
		}

		public override bool Used(ItemID item)
		{
			if (item != ItemID.GreenCrystal && item != ItemID.RedCrystal && item != ItemID.BlueCrystal)
				return false;

			Status = Status switch
			{
				AnimateStatus.Initial => AnimateStatus.FirstStep,
				AnimateStatus.FirstWait => AnimateStatus.SecondStep,
				_ => AnimateStatus.Operating,
			};
			return true;
		}
	}
}
