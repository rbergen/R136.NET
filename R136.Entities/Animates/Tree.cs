using R136.Entities.General;
using System;
using System.Collections.Generic;

namespace R136.Entities.Animates
{
	class Tree : Animate
	{
		event Action? Burned;

		public Tree(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public override ICollection<string>? ProgressStatus()
		{
			var texts = new List<string>();

			var statusTexts = GetTextsForStatus(Status);
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

				Burned?.Invoke();

				Status = AnimateStatus.Done;
			}

			return texts.Count > 0 ? texts : null;
		}

		public override Result Used(ItemID item)
		{
			if (item != ItemID.Flamethrower)
				return Result.Error();

			Status = AnimateStatus.Operating;

			return Result.Success();
		}
	}
}
