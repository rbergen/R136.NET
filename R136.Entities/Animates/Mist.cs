using R136.Entities.Global;

namespace R136.Entities.Animates
{
	class Mist : Animate
	{
		public Mist(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

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
