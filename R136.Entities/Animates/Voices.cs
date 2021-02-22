using R136.Entities.Global;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	class Voices : Animate
	{
		public static Animate Create(Initializer initializer)
			=> new Voices(initializer.ID, initializer.StartRoom);

		private Voices(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (!Facilities.Configuration.FreezeAnimates && status == AnimateStatus.Initial)
				Status = AnimateStatus.Done;
		}
	}
}
