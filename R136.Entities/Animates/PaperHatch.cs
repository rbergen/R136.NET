using R136.Entities.General;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	class PaperHatch : Animate, ITriggerable
	{
		public static Animate FromInitializer(Initializer initializer)
			=> new PaperHatch(initializer.ID, initializer.StartRoom);

		private PaperHatch(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (status == AnimateStatus.Operating)
			{
				StatusManager?.PutDown(ItemID.Paper);
				Status = AnimateStatus.Done;
			}
		}

		public void Trigger()
		{
			Status = AnimateStatus.Operating;
		}
	}
}
