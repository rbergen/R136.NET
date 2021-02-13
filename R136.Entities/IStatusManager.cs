using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities
{
	public enum HealthSeverity
	{
		Normal,
		Serious
	}

	public interface IStatusManager
	{
		public void DecreaseHealth();

		public void DecreaseHealth(HealthSeverity severity);

		public RoomID CurrentRoomID { get; set; }
		
		public bool IsRoomDark { get; }

		//		public bool IsInPosession(Item item);

		public bool EndRequested { get; set; }
	}
}
