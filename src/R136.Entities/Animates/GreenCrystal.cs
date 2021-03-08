using Microsoft.Extensions.DependencyInjection;
using R136.Entities.Global;
using R136.Interfaces;
using System;

namespace R136.Entities.Animates
{
	public class GreenCrystal : Animate, IGameServiceBasedConfigurator
	{
		public static Animate Create(Initializer initializer)
			=> new GreenCrystal(initializer.ID, initializer.StartRoom);

		private GreenCrystal(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (Facilities.Configuration.AutoPlaceItems)
				StatusManager?.Place(ItemID.GreenCrystal);
		}

		protected override void Trigger()
			=>	Status = AnimateStatus.Done;

		public void Configure(IServiceProvider serviceProvider)
		{
			var burnedNotifier = serviceProvider.GetService<IFireNotificationProvider>();

			if (burnedNotifier != null)
				burnedNotifier.Burned += Trigger;
		}
	}
}
