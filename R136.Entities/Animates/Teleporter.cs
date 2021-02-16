namespace R136.Entities.Animates
{
	public class Teleporter : Animate
	{
		public Teleporter(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public override void ProgressStatusInternal(AnimateStatus status)
		{
			if (StatusManager != null)
			{
				StatusManager.CurrentRoom = RoomID.Forest1;
			}
		}
	}
}
