using R136.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Animates
{
	public class Swelling : Animate
	{
		public static StatusTextMapper? StatusTexts { get; set; }

		public Swelling(AnimateID id, RoomID startRoom) : base(id, startRoom, StatusTexts) { }

		public override ICollection<string>? ProcessStatus()
		{
			AnimateStatus textStatus = Status;

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

		public override bool Used(ItemID item)
		{
			if (item != ItemID.GasGrenade)
				return false;

			Status = AnimateStatus.Dying;
			return true;
		}
	}
}
