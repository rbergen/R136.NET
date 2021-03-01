using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Shell
{
	class GameConsole
	{
		private readonly IServiceProvider _serviceProvider;

		public GameConsole(IServiceProvider serviceProvider)
			=> _serviceProvider = serviceProvider;

		public int Play()
		{
			return 0;
		}
	}
}
