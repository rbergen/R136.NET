using Microsoft.Extensions.DependencyInjection;

namespace R136.Interfaces
{
	public interface IGameServiceProvider
	{
		void RegisterServices(IServiceCollection serviceCollection);
	}
}
