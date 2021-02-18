using R136.Entities.General;
using R136.Entities.Global;
using System;

namespace R136.Entities.Animates
{
	class Gnu : Animate, INotifyRoomChangeRequested
	{
		public Gnu(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			switch (status)
			{
				case AnimateStatus.Initial:
					Status = AnimateStatus.Attack;

					break;

				case AnimateStatus.Attack:
					StatusManager?.DecreaseHealth();

					break;

				case AnimateStatus.Dying:
					StatusManager?.PutDown(ItemID.RedCrystal);
					Status = AnimateStatus.Done;

					break;
			}
		}

		public override Result Used(ItemID item)
		{
			if (item != ItemID.Pornbook)
				return Result.Failure();

			Status = AnimateStatus.Dying;
			return Result.Success();
		}

		public bool RoomChangeRequested(RoomID from, RoomID to)
		{
			return true;
		}

		public void RoomChanged(RoomID from, RoomID to) 
		{
			if (from == CurrentRoom && Status != AnimateStatus.Done)
				CurrentRoom = Facilities.Configuration.GnuRoamingRooms[Facilities.Randomizer.Next(5)];
		}
	}
}
