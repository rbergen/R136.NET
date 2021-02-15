using R136.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Animates
{
	public class Swamp : Animate
	{
		public static StatusTextMapper? StatusTexts { get; set; }

		public Swamp(AnimateID id, RoomID startRoom) : base(id, startRoom, StatusTexts) { }

		public override void ProcessStatusInternal(AnimateStatus status)
		{
			if (StatusManager == null)
				return;

			switch (ID)
			{
				case AnimateID.NorthSwamp:
					StatusManager.CurrentRoom = RoomID.EmptyCave51;

					break;

				case AnimateID.MiddleSwamp:
					StatusManager.CurrentRoom = RoomID.GloomyCave;

					break;

				case AnimateID.SouthSwamp:
					StatusManager.CurrentRoom = RoomID.RockCave;

					break;
			}
		}
	}
}
