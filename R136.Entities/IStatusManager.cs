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
		public void DecreaseHealth(HealthImpact impact);
		public void RestoreHealth();

		public int LifePoints { get; }
		public int? LampPoints { get; set; }

		public RoomID CurrentRoom { get; set; }
		
		public bool IsInPosession(ItemID item);
		public void RemoveFromPossession(ItemID item);

		public void PutDown(ItemID item);
		public void OpenConnection(Direction direction, RoomID toRoom);

		public void StartForestFire();

		public bool RequestEnd { get; set; }
	}
}
