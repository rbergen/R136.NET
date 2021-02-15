using R136.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Animates
{
	public class Voices : Animate
	{
		public static StatusTextMapper? StatusTexts { get; set; }

		public Voices(AnimateID id, RoomID startRoom) : base(id, startRoom, StatusTexts) { }

		public override void ProcessStatusInternal(AnimateStatus status)
		{
			if (status == AnimateStatus.Initial)
				Status = AnimateStatus.Done;
		}
	}
}
