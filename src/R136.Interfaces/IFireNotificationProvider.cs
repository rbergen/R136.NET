using System;

namespace R136.Interfaces
{
	public interface IFireNotificationProvider
	{
		public event Action? Burned;
	}
}
