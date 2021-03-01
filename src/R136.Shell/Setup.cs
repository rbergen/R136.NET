using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using R136.Core;
using R136.Shell.Tools;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }

  }
}
