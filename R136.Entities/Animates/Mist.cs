using R136.Entities.Global;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	class Mist : Animate
	{
		public static Animate Create(Initializer initializer)
			=> new Mist(initializer.ID, initializer.StartRoom);

		private Mist(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (StatusManager != null)
			{
				switch (Facilities.Randomizer.Next(3))
				{
					case 0:
						StatusManager.CurrentRoom = RoomID.StormCave;
						break;
					case 1:
						StatusManager.CurrentRoom = RoomID.SmallCave;
						break;
					case 2:
						StatusManager.CurrentRoom = RoomID.SpiralstaircaseCave1;
						break;
				}
			}
		}
	}
}
