using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using R136.Entities.Global;
using R136.Interfaces;

namespace R136.Core
{
	public class HttpJsonEntityReader : IEntityReader
	{
		private readonly HttpClient _client;

		public HttpJsonEntityReader(Uri baseUri)
		{
			_client = new HttpClient() { BaseAddress = baseUri };
			LogLine($"created with base URI {_client.BaseAddress}");
		}

		public async Task<TEntity?> ReadEntity<TEntity>(string? groupLabel, string label)
		{
			try
			{
				var fullLabel = groupLabel != null ? $"{groupLabel}/{label}" : label;

				LogLine($"loading {fullLabel}...");

				var result = JsonSerializer.Deserialize<TEntity>(await _client.GetStringAsync($"{fullLabel}.json"));

				LogLine($"{fullLabel} loaded successfully");

				return result;
			}
			catch (HttpRequestException e)
			{
				LogLine($"loading of {label} failed with exception {e}");
				return default;
			}
			catch (TaskCanceledException e)
			{
				LogLine($"loading of {label} failed with exception {e}");
				return default;
			}
		}

		private void LogLine(string text)
		{
			Facilities.LogLine(this, text);
		}
	}
}

#nullable restore
