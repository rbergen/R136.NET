namespace R136.Entities.Animates
{
	public class DragonHead : Animate
	{
		public DragonHead(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public override void ProgressStatusInternal(AnimateStatus status)
		{
			switch (status)
			{
				case AnimateStatus.FirstStep:
					Status = AnimateStatus.FirstWait;

					break;

				case AnimateStatus.SecondStep:
					Status = AnimateStatus.SecondWait;

					break;

				case AnimateStatus.Operating:
					StatusManager?.OpenConnection(Direction.North, RoomID.MainCave);
					Status = AnimateStatus.Done;

					break;
			}
		}

		public override bool Used(ItemID item)
		{
			if (item != ItemID.GreenCrystal && item != ItemID.RedCrystal && item != ItemID.BlueCrystal)
				return false;

			Status = Status switch
			{
				AnimateStatus.Initial => AnimateStatus.FirstStep,
				AnimateStatus.FirstWait => AnimateStatus.SecondStep,
				_ => AnimateStatus.Operating,
			};
			return true;
		}
	}
}
