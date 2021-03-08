using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Interfaces
{
	public interface IGameServiceProvider
	{
		void RegisterServices(IServiceCollection serviceCollection);
	}
}
