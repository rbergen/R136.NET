using R136.Interfaces;

namespace R136.Entities.Animates
{
	class Voices : Animate
	{
		public Voices(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (status == AnimateStatus.Initial)
				Status = AnimateStatus.Done;
		}
	}
}
