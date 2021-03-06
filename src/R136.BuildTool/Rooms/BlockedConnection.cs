using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.BuildTool.Rooms
{
	class BlockedConnection
	{
		public RoomID Room { get; set; }
		public Direction Direction { get; set; }
	}
}
