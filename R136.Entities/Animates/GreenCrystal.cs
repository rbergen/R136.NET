using R136.Entities.General;
using R136.Entities.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Animates
{
	public class GreenCrystal : Animate, ITriggerable
	{
		public static StatusTextMapper? StatusTexts { get; set; }

		public GreenCrystal(IServiceProvider serviceProvider, RoomID startRoom) : base(serviceProvider, startRoom, StatusTexts) { }

		public void Trigger()
		{
			Status = AnimateStatus.Done;
		}
	}
}
