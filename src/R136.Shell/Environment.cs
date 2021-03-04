﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using R136.Core;
using R136.Shell.Tools;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace R136.Shell
{
	static class Environment
	{
    public static IServiceProvider Setup(string[] args)
      => RegisterServices(AppDomain.CurrentDomain.BaseDirectory, args);

    private static IConfiguration SetupConfiguration(string basePath, string[] args)
    {
      return new ConfigurationBuilder()
          .SetBasePath(basePath)
          .AddJsonFile("appsettings.json")
          .AddEnvironmentVariables()
          .AddCommandLine(args)
          .Build();
    }

    private static ServiceProvider RegisterServices(string basePath, string[] args)
    {
      var configuration = SetupConfiguration(basePath, args);
      var serviceCollection = new ServiceCollection();

      var logger = new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();

      serviceCollection
        .AddSingleton(configuration)
        .AddLogging(loggingBuilder => loggingBuilder.AddSerilog(logger, true))
        .AddSingleton(SetupStatus)
        .AddSingleton<ILanguageProvider>(sp => new LanguageProvider() { Services = sp })
        .AddR136(sp => new FileSystemJsonEntityReader(sp, Path.Combine(basePath, "data")));

      var serviceProvider = serviceCollection.BuildServiceProvider();
      serviceProvider
        .PreLoadR136Async(configuration
          .GetSection(Constants.Languages)
          .GetChildren()
          .Select(section => section.Key)
          .ToArray()
         );

      return serviceProvider;

      Status SetupStatus(IServiceProvider serviceProvider)
			{
        Status? status = null;

        if (configuration["load"] != "no")
          status = Status.Load(serviceProvider);

        if (status == null)
          status = new();

        var language = configuration["lang"];
        if (language != null)
          status.Language = language;

        return status;
      }
    }

  }
}
