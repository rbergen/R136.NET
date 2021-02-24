using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using R136.Core;
using R136.Web.Tools;
using System;
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
			.AddScoped(sp => new HttpClient { BaseAddress = baseUri })
			.AddSingleton(new MarkupContentLog()
			{
				MaxBlockCount = builder.Configuration.GetValue<int>(Constants.MaxContentLogBlockCount),
				SaveBlockCount = builder.Configuration.GetValue<int>(Constants.SaveContentLogBlockCount)
			})
			.AddR136(() => new Uri(baseUri, "data/"))
			.AddBlazoredLocalStorage();

			var host = builder.Build();

			await host.Services.InitializeR136Async();
			await host.RunAsync();
		}
	}
}
