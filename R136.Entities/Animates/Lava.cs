using System.Collections.Generic;

namespace R136.Entities.Animates
{
	public class Lava : Animate
	{
		public Lava(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public override ICollection<string>? ProgressStatus()
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
