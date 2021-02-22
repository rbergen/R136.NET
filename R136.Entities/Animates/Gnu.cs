using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	class Gnu : Animate, INotifyRoomChangeRequested
	{
		public static Animate Create(Initializer initializer)
			=> new Gnu(initializer.ID, initializer.StartRoom);

		private Gnu(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (Facilities.Configuration.AutoPlaceItems)
				StatusManager?.Place(ItemID.RedCrystal);

			switch (status)
			{
				case AnimateStatus.Initial:
					if (!Facilities.Configuration.FreezeAnimates)
						Status = AnimateStatus.Attack;

					break;

				case AnimateStatus.Attack:
					StatusManager?.DecreaseHealth();

					break;

				case AnimateStatus.Dying:
					StatusManager?.Place(ItemID.RedCrystal);
					Status = AnimateStatus.Done;

					break;
			}
		}

		public override Result ApplyItem(ItemID item)
		{
			if (item != ItemID.Pornbook)
				return Result.Failure();

			Status = AnimateStatus.Dying;
			return Result.Success();
		}

		public bool RoomChangeRequested(RoomID from, RoomID to) => true;

		public void RoomChanged(RoomID from, RoomID to) 
		{
			if (from == CurrentRoom && Status != AnimateStatus.Done)
				CurrentRoom = Facilities.Configuration.GnuRoamingRooms[Facilities.Randomizer.Next(5)];
		}
	}
}
