using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	class PaperHatch : Animate, ITriggerable
	{
		public static Animate Create(Initializer initializer)
			=> new PaperHatch(initializer.ID, initializer.StartRoom);

		private PaperHatch(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (Facilities.Configuration.AutoPlaceItems)
				StatusManager?.Place(ItemID.Paper);

			if (status == AnimateStatus.Operating)
			{
				StatusManager?.Place(ItemID.Paper);
				Status = AnimateStatus.Done;
			}
		}

		public void Trigger()
		{
			if (Status == AnimateStatus.Initial)
				Status = AnimateStatus.Operating;

			IsTriggered = true;
		}
	}
}
