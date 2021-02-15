using R136.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Animates
{
	public class Lava : Animate
	{
		public static StatusTextMapper? StatusTexts { get; set; }

		public Lava(AnimateID id, RoomID startRoom) : base(id, startRoom, StatusTexts) { }

		public override ICollection<string>? ProcessStatus()
		{
			AnimateStatus textStatus = Status;

			switch (textStatus)
			{
				case AnimateStatus.Initial:
					if (StatusManager != null && !StatusManager.IsInPosession(ItemID.HeatSuit))
					{
						textStatus = AnimateStatus.SelfInjury;
						StatusManager.DecreaseHealth(HealthImpact.Severe);
						StatusManager.CurrentRoom = RoomID.OilCave;
					}

					break;

				case AnimateStatus.Operating:
					if (StatusManager != null)
						StatusManager.RequestEnd = true;

					break;
			}

			return GetTextsForStatus(textStatus);
		}

		public override bool Used(ItemID item)
		{
			if (item != ItemID.Bomb)
				return false;

			Status = AnimateStatus.Operating;

			return true;
		}
	}
}
