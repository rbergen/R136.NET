using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using R136.Interfaces;

#nullable enable

namespace R136.Web.Tools
{
	public class JsonEntityReader : IEntityReader
	{
		private readonly HttpClient _client;

		public JsonEntityReader(Uri baseUri) => _client = new HttpClient() { BaseAddress = baseUri };

		public async Task<TEntity?> ReadEntity<TEntity>(string label)
		{
			try
			{
				return await JsonSerializer.DeserializeAsync<TEntity>(await _client.GetStreamAsync($"{label}.json")).AsTask();
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
