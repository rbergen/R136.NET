using Microsoft.Extensions.DependencyInjection;
using R136.Entities.Global;
using R136.Interfaces;

namespace R136.Entities
{
	public abstract class EntityBase	{
		private static IStatusManager? _statusManager = null;
		private readonly object _statusManagerLock = new object();

		protected IStatusManager? StatusManager
		{
			get
			{
				lock (_statusManagerLock)
				{
					if (_statusManager == null && Facilities.Services != null)
						_statusManager = Facilities.Services.GetService<IStatusManager>();

					return _statusManager;
				}
			}
		}
	}
}
