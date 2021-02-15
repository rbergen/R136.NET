using R136.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Animates
{
	public class Door : Animate
	{
		public static StatusTextMapper? StatusTexts { get; set; }

		public Door(AnimateID id, RoomID startRoom) : base(id, startRoom, StatusTexts) { }

		public override void ProcessStatusInternal(AnimateStatus status)
		{
			if (status == AnimateStatus.Operating)
			{
				StatusManager?.OpenConnection(Direction.North, RoomID.GarbageCave);
				Status = AnimateStatus.Done;
			}
		}

		public override bool Used(ItemID item)
		{
			if (item != ItemID.Bone)
				return false;

			Status = AnimateStatus.Operating;
			return true;
		}
	}
} 

