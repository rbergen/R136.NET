using Microsoft.Extensions.DependencyInjection;
using R136.Entities.Global;
using R136.Interfaces;
using System;

namespace R136.Core
{
	public static class Config
	{
		public static IServiceCollection AddR136(this IServiceCollection serviceCollection, Func<Uri> baseUriProvider)
		 => AddR136(serviceCollection, new HttpJsonEntityReader(baseUriProvider.Invoke()));

		public static IServiceCollection AddR136(this IServiceCollection serviceCollection, Func<IEntityReader> entityReaderProvider)
		 => AddR136(serviceCollection, entityReaderProvider.Invoke());

		private static IServiceCollection AddR136(IServiceCollection serviceCollection, IEntityReader? reader = null)
		{
			Facilities.Configuration.LogToConsole = true;

			if (reader != null)
				serviceCollection.AddSingleton(reader);

			var engine = new Engine();

			return serviceCollection
				.AddSingleton<IEngine>(sp => 
				{
					engine.Services = sp;

					return engine;
				})
				.AddSingleton(engine.StatusManager);
		}

		public static void PreLoadR136Async(this IServiceProvider serviceProvider, string[] entityGroups)
			=> serviceProvider.GetRequiredService<IEngine>().StartLoadEntities(entityGroups);
	}
}
