using R136.Entities.General;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	public class GreenCrystal : Animate, ITriggerable
	{
		public static Animate FromInitializer(Initializer initializer)
			=> new GreenCrystal(initializer.ID, initializer.StartRoom);

		private GreenCrystal(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public void Trigger()
		{
			Status = AnimateStatus.Done;
		}
	}
}
