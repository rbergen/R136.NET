using R136.Entities.Global;
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

		public HttpJsonEntityReader(Uri baseUri)
			=> _client = new HttpClient() { BaseAddress = baseUri };

		public async Task<TEntity?> ReadEntity<TEntity>(string? groupLabel, string label)
		{
			try
			{
				var fullLabel = groupLabel != null ? $"{groupLabel}/{label}" : label;

				return JsonSerializer.Deserialize<TEntity>(await _client.GetStringAsync($"{fullLabel}.json"));
			}
			catch (HttpRequestException)
			{
				return default;
			}
			catch (TaskCanceledException)
			{
				return default;
			}
		}
	}
}

#nullable restore
