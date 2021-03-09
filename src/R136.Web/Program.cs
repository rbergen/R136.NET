using Blazor.Extensions.Logging;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R136.Core;
using R136.Web.Tools;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace R136.Web
{
	public class Program
	{
		public static async Task Main(string[] args)
		{
			var builder = WebAssemblyHostBuilder.CreateDefault(args);
			builder.RootComponents.Add<App>("#app");

			var baseUri = new Uri(builder.HostEnvironment.BaseAddress);

			builder.Services
				.AddLogging
				(	builder => builder
					.AddBrowserConsole()
					.SetMinimumLevel(LogLevel.Debug)
				)
				.AddScoped(sp => new HttpClient { BaseAddress = baseUri })
				.AddSingleton(new MarkupContentLog()
				{
					MaxBlockCount = builder.Configuration.GetValue<int>(Constants.MaxContentLogBlockCount),
					SaveBlockCount = builder.Configuration.GetValue<int>(Constants.SaveContentLogBlockCount)
				})
				.AddR136(sp => new HttpJsonEntityReader(sp, new Uri(baseUri, "data/")))
				.AddBlazoredLocalStorage()
				.AddScoped<ILanguageProvider>(sp => new LanguageProvider() { Services = sp });

			var host = builder.Build();

			host.Services.PreLoadR136Async
			(	builder
				.Configuration
				.GetSection(Constants.Languages)
				.GetChildren()
				.Select(section => section.Key)
				.ToArray()
			);

			await host.RunAsync();
		}
	}
}
