using R136.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Animates
{
	public class Tree : Animate
	{
		public static StatusTextMapper? StatusTexts { get; set; }

		public Tree(IServiceProvider serviceProvider, RoomID startRoom) : base(serviceProvider, startRoom, StatusTexts) { }

		public override ICollection<string>? ProcessStatus()
		{
			List<string> texts = new List<string>();

			ICollection<string>? statusTexts = GetTextsForStatus(Status);
			if (statusTexts != null)
				texts.AddRange(statusTexts);

			if (Status == AnimateStatus.Operating)
			{
				if (!(StatusManager?.IsItemInPosession(ItemID.HeatSuit) ?? false))
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
