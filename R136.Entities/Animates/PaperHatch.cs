using R136.Entities.General;

namespace R136.Entities.Animates
{
	public class PaperHatch : Animate, ITriggerable
	{

		public PaperHatch(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public override void ProgressStatusInternal(AnimateStatus status)
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
