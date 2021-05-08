using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R136.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace R136.Shell.Tools
{
	public class FileSystemJsonEntityReader : IEntityReader
	{
		private readonly string basePath;
		private readonly ILogger<FileSystemJsonEntityReader>? logger;

		public FileSystemJsonEntityReader(IServiceProvider services, string basePath)
		{
			this.logger = services.GetService<ILogger<FileSystemJsonEntityReader>>();
			this.basePath = basePath;
			this.logger?.LogDebug($"created with base URI {this.basePath}");
		}

		public async Task<TEntity?> ReadEntity<TEntity>(string? groupLabel, string label)
		{
			try
			{
				string fileName = $"{label}.json";
				string jsonFilePath = groupLabel != null ? Path.Join(this.basePath, groupLabel, fileName) : Path.Join(this.basePath, fileName);
				this.logger?.LogDebug($"loading {groupLabel}.{label}...");

				var result = JsonSerializer.Deserialize<TEntity>(await File.ReadAllTextAsync(jsonFilePath, Encoding.UTF8));
				this.logger?.LogDebug($"{groupLabel}.{label} loaded successfully");

				return result;
			}
			catch (Exception e)
			{
				this.logger?.LogDebug($"loading of {groupLabel}.{label} failed with exception {e}");

				return default;
			}
		}
	}
}
