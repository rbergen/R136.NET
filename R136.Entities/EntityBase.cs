using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using R136.Entities.Global;

namespace R136.Entities
{
	enum EntityType
	{
		Animate,
		Item,
		Room,
		Action,
		Text
	}

	public class EntityBase
	{
		private IStatusManager? _statusManager = null;

		protected IStatusManager? StatusManager
		{
			get
			{
				if (_statusManager == null && Facilities.ServiceProvider != null)
				{
					_statusManager = Facilities.ServiceProvider.GetService<IStatusManager>();
				}

				return _statusManager;
			}
		}
	}
}
