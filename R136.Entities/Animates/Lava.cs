using R136.Entities.General;
using System.Collections.Generic;

namespace R136.Entities.Animates
{
	class Lava : Animate
	{
		public Lava(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public override ICollection<string>? ProgressStatus()
		{
			var textStatus = Status;

			if (textStatus == AnimateStatus.Initial && StatusManager != null && !StatusManager.IsInPosession(ItemID.HeatSuit))
			{
				textStatus = AnimateStatus.SelfInjury;
				StatusManager.DecreaseHealth(HealthImpact.Severe);
				StatusManager.CurrentRoom = RoomID.OilCave;
			}

			return GetTextsForStatus(textStatus);
		}

		public override Result Used(ItemID item)
		{
			if (item != ItemID.Bomb)
				return Result.Error();

			Status = AnimateStatus.Operating;

			return Result.EndRequested();
		}
	}
}
