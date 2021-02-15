using R136.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Animates
{
	public class Vacuum : Animate
	{
		public static StatusTextMapper? StatusTexts { get; set; }

		public Vacuum(AnimateID id, RoomID startRoom) : base(id, startRoom, StatusTexts) { }

		public override void ProcessStatusInternal(AnimateStatus status)
		{
			if (StatusManager != null)
			{
				StatusManager.DecreaseHealth(HealthImpact.Severe);
				StatusManager.CurrentRoom = RoomID.SnakeCave;
			}
		}
	}
}
