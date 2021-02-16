using R136.Entities.Global;

namespace R136.Entities.Animates
{
	public class Gnu : Animate, INotifyRoomChangeRequested
	{
		RoomChangeRequestedHandler INotifyRoomChangeRequested.Handler => RoomChangeRequestedHandler;

		public Gnu(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

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

		public override bool Used(ItemID item)
		{
			if (item != ItemID.Pornbook)
				return false;

			Status = AnimateStatus.Dying;
			return true;
		}

		private bool RoomChangeRequestedHandler(object sender, RoomChangeRequestedEventArgs e)
		{
			if (e.From == CurrentRoom && Status != AnimateStatus.Done)
				CurrentRoom = Facilities.Configuration.GnuRoamingRooms[Facilities.Randomizer.Next(5)];

			return true;
		}
	}
}
