using R136.Entities.Global;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	class Vacuum : Animate
	{
		public static Animate Create(Initializer initializer)
			=> new Vacuum(initializer.ID, initializer.StartRoom);

		private Vacuum(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (!Facilities.Configuration.FreezeAnimates && StatusManager != null)
			{
				StatusManager.DecreaseHealth(HealthImpact.Severe);
				StatusManager.CurrentRoom = RoomID.SnakeCave;
			}
		}
	}
}
