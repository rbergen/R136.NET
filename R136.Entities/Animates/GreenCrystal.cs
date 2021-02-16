using R136.Entities.General;

namespace R136.Entities.Animates
{
	public class GreenCrystal : Animate, ITriggerable
	{
		public GreenCrystal(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public void Trigger()
		{
			Status = AnimateStatus.Done;
		}
	}
}
