using System;

namespace R136.Interfaces
{
	public interface IPaperRouteNotificationProvider
	{
		public event Action? PaperRouteCompleted;
	}
}
