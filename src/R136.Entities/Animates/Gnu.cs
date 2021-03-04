﻿using R136.Entities.General;
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
					Status = AnimateStatus.Done;

					break;
			}
		}

		public override Result ApplyItem(ItemID item)
		{
			if (item != ItemID.PoisonedMeat)
				return Result.Failure();

			Status = AnimateStatus.Dying;
			IsTriggered = true;
			return Result.Success();
		}


		public void RoomChanged(RoomChangeRequestedEventArgs e) 
		{
			if (e.From == CurrentRoom && Status != AnimateStatus.Done)
			{
				var roamingRooms = Facilities.Configuration.GnuRoamingRooms;
				CurrentRoom = roamingRooms[Facilities.Randomizer.Next(roamingRooms.Length)]; 
			}
		}

		public void RoomChangeRequested(RoomChangeRequestedEventArgs args) { }
	}
}