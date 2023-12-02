using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using R136.Core;
using R136.Shell.Tools;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace R136.Shell
{
	static class Environment
	{
		public static IServiceProvider Setup(string[] args)
			=> RegisterServices(AppDomain.CurrentDomain.BaseDirectory, args);

		private static IConfiguration SetupConfiguration(string basePath, string[] args)
			=> new ConfigurationBuilder()
				.SetBasePath(basePath)
				.AddJsonFile("appsettings.json")
				.AddCommandLine(args)
				.Build();

		private static ServiceProvider RegisterServices(string basePath, string[] args)
		{
			var configuration = SetupConfiguration(basePath, args);
			var serviceCollection = new ServiceCollection();

			var logger = new LoggerConfiguration()
				.ReadFrom.Configuration(configuration)
				.CreateLogger();

			var serviceProvider = serviceCollection
				.AddSingleton(configuration)
				.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger, true))
				.AddSingleton(SetupStatus)
				.AddSingleton<ILanguageProvider>(sp => new LanguageProvider() { Services = sp })
				.AddR136(sp => new FileSystemJsonEntityReader(sp, Path.Combine(basePath, "data")))
				.BuildServiceProvider();

			serviceProvider.PreLoadR136Async(GetAvailableLanguages().ToArray());

			return serviceProvider;

			Status SetupStatus(IServiceProvider serviceProvider)
			{
				Status? status = null;

				if (configuration[Constants.LoadParam] != Constants.ParamNo)
					status = Status.Load(serviceProvider);

				if (status == null)
					status = new();

				string? language = configuration[Constants.LanguageParam];

				if (language != null && GetAvailableLanguages().Contains(language))
					status.Language = language;

				return status;
			}

			IEnumerable<string> GetAvailableLanguages() 
				=> configuration
					.GetSection(Constants.Languages)
					.GetChildren()
					.Select(section => section.Key);
		}

	}
}
