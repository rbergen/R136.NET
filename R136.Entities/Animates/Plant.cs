using R136.Entities.Global;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	class Plant : StrikableAnimate
	{
		public Plant(AnimateID id, RoomID startRoom, int strikeCount) : base(id, startRoom, strikeCount) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			switch (status)
			{
				case AnimateStatus.Initial:
				case AnimateStatus.Attack:
					StatusManager?.DecreaseHealth();

					Status = Facilities.Randomizer.Next(2) == 0
						? AnimateStatus.PreparingNextAttack
						: AnimateStatus.Attack;

					break;

				case AnimateStatus.PreparingNextAttack:
					Status = AnimateStatus.Attack;

					break;

				case AnimateStatus.Dying:
					StatusManager?.OpenConnection(Direction.North, RoomID.SlimeCave);
					Status = AnimateStatus.Done;

					break;
			}
		}
	}
}
