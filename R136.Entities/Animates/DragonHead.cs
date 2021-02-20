using R136.Interfaces;

namespace R136.Entities.Animates
{
	class DragonHead : Animate
	{
		public static Animate FromInitializer(Initializer initializer)
			=> new DragonHead(initializer.ID, initializer.StartRoom);

		private DragonHead(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
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

		public override Result Used(ItemID item)
		{
			if (item != ItemID.GreenCrystal && item != ItemID.RedCrystal && item != ItemID.BlueCrystal)
				return Result.Error();

			Status = Status switch
			{
				AnimateStatus.Initial => AnimateStatus.FirstStep,
				AnimateStatus.FirstWait => AnimateStatus.SecondStep,
				_ => AnimateStatus.Operating,
			};

			return Result.Success();
		}
	}
}
