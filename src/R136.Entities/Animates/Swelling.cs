using Microsoft.Extensions.Primitives;
using R136.Entities.Global;
using R136.Interfaces;

namespace R136.Entities.Animates
{
	class Swelling : Animate
	{
		public static Animate Create(Initializer initializer)
			=> new Swelling(initializer.ID, initializer.StartRoom);

		private Swelling(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public override StringValues ProgressStatus()
		{
			if (Facilities.Configuration.AutoOpenConnections)
				StatusManager?.OpenConnection(Direction.North, RoomID.DamnationCave);

			var textStatus = Status;

			switch (textStatus)
			{
				case AnimateStatus.Initial:
					if (!Facilities.Configuration.FreezeAnimates)
						Status = AnimateStatus.FirstWait;

					break;

				case AnimateStatus.Dying:
					if (!(Player?.IsInPosession(ItemID.GasMask) ?? false))
						textStatus = AnimateStatus.SelfInjury;

					StatusManager?.OpenConnection(Direction.North, RoomID.DamnationCave);
					Status = AnimateStatus.Done;

					break;
			}

			return GetTextsForStatus(textStatus);
		}

		public override Result ApplyItem(ItemID item)
		{
			if (item != ItemID.GasGrenade)
				return Result.Error();

			Status = AnimateStatus.Dying;
			IsTriggered = true;
			return Result.Success();
		}
	}
}
