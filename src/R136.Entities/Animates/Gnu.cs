﻿using Microsoft.Extensions.DependencyInjection;
using R136.Entities.Global;
using R136.Interfaces;
using System;

namespace R136.Entities.Animates
{
	class Gnu : Animate, IGameServiceBasedConfigurator
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
					Player?.DecreaseHealth();

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

			Trigger();

			return Result.Success();
		}

		public void RoomChangedHandler(RoomChangeRequestedEventArgs e)
		{
			if (e.From == CurrentRoom && Status != AnimateStatus.Done)
			{
				var roamingRooms = Facilities.Configuration.GnuRoamingRooms;
				CurrentRoom = roamingRooms[Facilities.Randomizer.Next(roamingRooms.Length)];
			}
		}

		public void Configure(IServiceProvider serviceProvider)
		{
			var roomChangeNotifier = serviceProvider.GetService<IRoomChangeNotificationProvider>();

			if (roomChangeNotifier != null)
				roomChangeNotifier.RoomChanged += RoomChangedHandler;
		}
	}
}
