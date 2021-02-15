using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities
{
	public enum HealthImpact
	{
		Normal,
		Severe
	}

	public interface IStatusManager
	{
		public void DecreaseHealth();

		public void DecreaseHealth(HealthImpact severity);

		public RoomID CurrentRoom { get; set; }
		
		public bool IsRoomDark { get; }

		public bool IsItemInPosession(ItemID item);
		public void ReleaseItem(ItemID item);
		
		public void OpenConnection(Direction direction, RoomID toRoom);

		public void StartForestFire();
		public bool IsForestBurned { get;	}

		public bool RequestEnd { get; set; }
	}
}
