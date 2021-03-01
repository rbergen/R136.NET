using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using R136.Core;
using R136.Shell.Tools;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace R136.Shell
{
  class Program
  {
    static void Main(string[] args)
    {
      var serviceProvider = RegisterServices(AppDomain.CurrentDomain.BaseDirectory, args);

      
    }

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
      IConfiguration configuration = SetupConfiguration(basePath, args);
      var serviceCollection = new ServiceCollection();

      serviceCollection
        .AddSingleton(configuration)
        .AddR136(new FileSystemJsonEntityReader(Path.Combine(basePath, "data")));

      var serviceProvider = serviceCollection.BuildServiceProvider();
      serviceProvider.PreLoadR136Async(configuration
        .GetSection(Constants.Languages)
        .GetChildren()
        .Select(section => section.Key)
        .ToArray()
       );

      return serviceProvider;
    }
  }
}
