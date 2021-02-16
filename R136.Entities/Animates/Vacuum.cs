namespace R136.Entities.Animates
{
	public class Vacuum : Animate
	{
		public Vacuum(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public override void ProgressStatusInternal(AnimateStatus status)
		{
			if (StatusManager != null)
			{
				StatusManager.DecreaseHealth(HealthImpact.Severe);
				StatusManager.CurrentRoom = RoomID.SnakeCave;
			}
		}
	}
}
