using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R136.Core;
using R136.Interfaces;
using R136.Web.Tools;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
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
			var engine = new Engine();

			builder.Services.AddScoped(sp => new HttpClient { BaseAddress = baseUri });
			builder.Services.AddSingleton(typeof(IEntityReader), new JsonEntityReader(new Uri(baseUri, "data")));
			builder.Services.AddSingleton(typeof(IEngine), engine);
			builder.Services.AddSingleton(typeof(IStatusManager), engine.StatusManager);

			var host = builder.Build();
			engine.Services = host.Services;

			await host.RunAsync();
		}
	}
}
