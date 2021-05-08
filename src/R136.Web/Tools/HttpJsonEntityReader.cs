using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R136.Interfaces;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

#nullable enable

namespace R136.Web.Tools
{
	public class HttpJsonEntityReader : IEntityReader
	{
		private readonly HttpClient client;
		private readonly ILogger<HttpJsonEntityReader>? logger;

		public HttpJsonEntityReader(IServiceProvider services, Uri baseUri)
		{
			this.logger = services.GetService<ILogger<HttpJsonEntityReader>>();
			this.client = new() { BaseAddress = baseUri };
			this.logger?.LogDebug($"created with base URI {this.client.BaseAddress}");
		}

		public async Task<TEntity?> ReadEntity<TEntity>(string? groupLabel, string label)
		{
			string fullLabel = groupLabel != null ? $"{groupLabel}/{label}" : label;

			try
			{
				this.logger?.LogDebug($"loading {fullLabel}...");

				var result = JsonSerializer.Deserialize<TEntity>(await this.client.GetStringAsync($"{fullLabel}.json"));
				this.logger?.LogDebug($"{fullLabel} loaded successfully");

				return result;
			}
			catch (Exception e)
			{
				this.logger?.LogDebug($"loading of {fullLabel} failed with exception {e}");

				return default;
			}
		}
	}
}

#nullable restore
