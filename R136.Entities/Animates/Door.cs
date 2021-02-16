namespace R136.Entities.Animates
{
	public class Door : Animate
	{
		public Door(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public override void ProgressStatusInternal(AnimateStatus status)
		{
			if (status == AnimateStatus.Operating)
			{
				StatusManager?.OpenConnection(Direction.North, RoomID.GarbageCave);
				Status = AnimateStatus.Done;
			}
		}

		public override bool Used(ItemID item)
		{
			if (item != ItemID.Bone)
				return false;

			Status = AnimateStatus.Operating;
			return true;
		}
	}
}

