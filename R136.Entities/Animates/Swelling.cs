using R136.Entities.General;
using System.Collections.Generic;

namespace R136.Entities.Animates
{
	public class Swelling : Animate
	{
		public Swelling(AnimateID id, RoomID startRoom) : base(id, startRoom) { }

		public override ICollection<string>? ProgressStatus()
		{
			var textStatus = Status;

			switch (textStatus)
			{
				case AnimateStatus.Initial:
					Status = AnimateStatus.FirstWait;

					break;

				case AnimateStatus.Dying:
					if (!(StatusManager?.IsInPosession(ItemID.GasMask) ?? false))
						textStatus = AnimateStatus.SelfInjury;

					StatusManager?.OpenConnection(Direction.North, RoomID.DamnationCave);
					Status = AnimateStatus.Done;

					break;
			}

			return GetTextsForStatus(textStatus);
		}

		public override Result Used(ItemID item)
		{
			if (item != ItemID.GasGrenade)
				return Result.Failure();

			Status = AnimateStatus.Dying;
			return Result.Success();
		}
	}
}
