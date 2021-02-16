using System.Collections.Generic;

namespace R136.Entities.Animates
{
	public class Tree : Animate
	{
		public Tree(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public override ICollection<string>? ProgressStatus()
		{
			List<string> texts = new List<string>();

			ICollection<string>? statusTexts = GetTextsForStatus(Status);
			if (statusTexts != null)
				texts.AddRange(statusTexts);

			if (Status == AnimateStatus.Operating)
			{
				if (!(StatusManager?.IsInPosession(ItemID.HeatSuit) ?? false))
				{
					statusTexts = GetTextsForStatus(AnimateStatus.SelfInjury);
					if (statusTexts != null)
						texts.AddRange(statusTexts);
				}

				StatusManager?.StartForestFire();
				Status = AnimateStatus.Done;
			}

			return texts.Count > 0 ? texts : null;
		}

		public override bool Used(ItemID item)
		{
			if (item != ItemID.Flamethrower)
				return false;

			Status = AnimateStatus.Operating;

			return true;
		}
	}
}
