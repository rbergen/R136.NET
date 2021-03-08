using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Interfaces
{
	public interface IRoomChangeNotificationProvider
	{
		event Action<RoomChangeRequestedEventArgs>? RoomChanged;
		event Action<RoomChangeRequestedEventArgs>? RoomChangeRequested;
	}

	public class RoomChangeRequestedEventArgs : CancelEventArgs
	{
		public RoomID From { get; init; }
		public RoomID To { get; init; }

		public RoomChangeRequestedEventArgs() { }

		public RoomChangeRequestedEventArgs(RoomID from, RoomID to)
			=> (From, To) = (from, to);
	}

}
