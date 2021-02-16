namespace R136.Entities.Animates
{
	public class Voices : Animate
	{
		public Voices(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public override void ProgressStatusInternal(AnimateStatus status)
		{
			if (status == AnimateStatus.Initial)
				Status = AnimateStatus.Done;
		}
	}
}
