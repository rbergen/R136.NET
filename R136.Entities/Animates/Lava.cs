using R136.Entities.Global;
using R136.Interfaces;
using System.Collections.Generic;

namespace R136.Entities.Animates
{
	class Lava : Animate
	{
		public static Animate Create(Initializer initializer)
			=> new Lava(initializer.ID, initializer.StartRoom);

		private Lava(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public override ICollection<string>? ProgressStatus()
		{
			var textStatus = Status;

			if (!Facilities.Configuration.FreezeAnimates && textStatus == AnimateStatus.Initial 
				&& StatusManager != null && !StatusManager.IsInPosession(ItemID.HeatSuit))
			{
				textStatus = AnimateStatus.SelfInjury;
				StatusManager.DecreaseHealth(HealthImpact.Severe);
				StatusManager.CurrentRoom = RoomID.OilCave;
			}

			return GetTextsForStatus(textStatus);
		}

		public override Result ApplyItem(ItemID item)
		{
			if (item != ItemID.Bomb)
				return Result.Error();

			Status = AnimateStatus.Operating;
			IsTriggered = true;
			return Result.EndRequested();
		}
	}
}
