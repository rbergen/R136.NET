using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Livings
{
	public class Hellhound : StrikableMonster
	{
		public Hellhound(RoomID startRoom) : base(startRoom, 4) { }
	}
}
