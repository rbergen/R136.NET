using R136.Entities.Global;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	class Door : Animate
	{
		public static Animate Create(Initializer initializer)
			=> new Door(initializer.ID, initializer.StartRoom);

		private Door(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		protected override void ProgressStatusInternal(AnimateStatus status)
		{
			if (Facilities.Configuration.AutoOpenConnections)
				StatusManager?.OpenConnection(Direction.North, RoomID.GarbageCave);

			if (status == AnimateStatus.Operating)
			{
				StatusManager?.OpenConnection(Direction.North, RoomID.GarbageCave);
				Status = AnimateStatus.Done;
			}
		}

		public override Result ApplyItem(ItemID item)
		{
			if (item != ItemID.Bone)
				return Result.Error();

			Status = AnimateStatus.Operating;
			Trigger();
			return Result.Success();
		}
	}
}

