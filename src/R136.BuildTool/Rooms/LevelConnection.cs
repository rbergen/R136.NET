using R136.Interfaces;

namespace R136.BuildTool.Rooms
{
	class LevelConnection
	{
		public RoomID From { get; set; }
		public Direction Direction { get; set; }
		public RoomID To { get; init; }
	}

}
