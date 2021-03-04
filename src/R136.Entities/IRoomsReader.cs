using R136.Interfaces;
using System.Collections.Generic;

namespace R136.Entities
{
	public interface IRoomsReader
	{
		IReadOnlyDictionary<RoomID, Room>? Rooms { set; }
	}
}
