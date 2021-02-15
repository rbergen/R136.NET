using R136.Entities.General;
using R136.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Animates
{
	public class PaperHatch : Animate, ITriggerable
	{
		public static StatusTextMapper? StatusTexts { get; set; }

		public PaperHatch(AnimateID id, RoomID startRoom) : base(id, startRoom, StatusTexts) { }

		public override void ProcessStatusInternal(AnimateStatus status)
		{
			if (status == AnimateStatus.Operating)
			{
				StatusManager?.PutDown(ItemID.Paper);
				Status = AnimateStatus.Done;
			}
		}

		public void Trigger()
		{
			Status = AnimateStatus.Operating;
		}
	}
}
