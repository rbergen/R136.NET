using R136.Entities.General;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	class GreenCrystal : Animate, ITriggerable
	{
		public GreenCrystal(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public void Trigger()
		{
			Status = AnimateStatus.Done;
		}
	}
}
