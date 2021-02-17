using R136.Entities.General;
using R136.Entities.Global;
using System;

namespace R136.Entities.Animates
{
	public class Gnu : Animate, INotifyRoomChangeRequested
	{
		public Gnu(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public Func<RequestedRoomChange, bool> RoomChangeRequestedHandler => RoomChangeRequested;

		public override void ProgressStatusInternal(AnimateStatus status)
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

		private bool RoomChangeRequested(RequestedRoomChange change)
		{
			if (change.From == CurrentRoom && Status != AnimateStatus.Done)
				CurrentRoom = Facilities.Configuration.GnuRoamingRooms[Facilities.Randomizer.Next(5)];

			return true;
		}
	}
}
