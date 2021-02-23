using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	public class GreenCrystal : Animate, ITriggerable
	{
		public static Animate Create(Initializer initializer)
			=> new GreenCrystal(initializer.ID, initializer.StartRoom);

		private GreenCrystal(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (Facilities.Configuration.AutoPlaceItems)
				StatusManager?.Place(ItemID.GreenCrystal);
		}

		public void Trigger()
		{
			Status = AnimateStatus.Done;
		}
	}
}
