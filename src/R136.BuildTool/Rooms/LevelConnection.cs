using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.BuildTool.Rooms
{
	class LevelConnection
	{
		public RoomID From { get; set; }
		public Direction Direction { get; set; }
		public RoomID To { get; init; }
	}

}
