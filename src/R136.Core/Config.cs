using Microsoft.Extensions.DependencyInjection;
using R136.Interfaces;
using System;

namespace R136.Core
{
	public static class Config
	{
		public static IServiceCollection AddR136(this IServiceCollection serviceCollection, Func<IServiceProvider, IEntityReader> entityReaderFactory)
		{
			serviceCollection.AddSingleton(entityReaderFactory);
			AddR136(serviceCollection);
			return serviceCollection;
		}

		public static IServiceCollection AddR136(this IServiceCollection serviceCollection, IEntityReader entityReader)
		{
			serviceCollection.AddSingleton(entityReader);
			AddR136(serviceCollection);
			return serviceCollection;
		}

		private static IServiceCollection AddR136(IServiceCollection serviceCollection)
			=> serviceCollection.AddSingleton<IEngine>(sp => new Engine { ContextServices = sp });

		public static void PreLoadR136Async(this IServiceProvider serviceProvider, string[] entityGroups)
			=> serviceProvider.GetRequiredService<IEngine>().StartLoadEntities(entityGroups);
	}
}
