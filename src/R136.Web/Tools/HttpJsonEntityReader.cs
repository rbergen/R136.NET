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
		private readonly HttpClient _client;
		private readonly ILogger<HttpJsonEntityReader>? _logger;

		public HttpJsonEntityReader(IServiceProvider services, Uri baseUri)
		{
			_logger = services.GetService<ILogger<HttpJsonEntityReader>>();
			_client = new HttpClient() { BaseAddress = baseUri };
			_logger?.LogDebug($"created with base URI {_client.BaseAddress}");
		}

		public async Task<TEntity?> ReadEntity<TEntity>(string? groupLabel, string label)
		{
			var fullLabel = groupLabel != null ? $"{groupLabel}/{label}" : label;

			try
			{
				_logger?.LogDebug($"loading {fullLabel}...");

				var result = JsonSerializer.Deserialize<TEntity>(await _client.GetStringAsync($"{fullLabel}.json"));
				_logger?.LogDebug($"{fullLabel} loaded successfully");

				return result;
			}
			catch (Exception e)
			{
				_logger?.LogDebug($"loading of {fullLabel} failed with exception {e}");

				return default;
			}
		}
	}
}

#nullable restore
