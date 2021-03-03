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
		private readonly string _basePath;
		private readonly ILogger<FileSystemJsonEntityReader>? _logger;

		public FileSystemJsonEntityReader(IServiceProvider services, string basePath)
		{
			_logger = services.GetService<ILogger<FileSystemJsonEntityReader>>();
			_basePath = basePath;
			_logger?.LogDebug($"created with base URI {_basePath}");
		}

		public async Task<TEntity?> ReadEntity<TEntity>(string? groupLabel, string label)
		{
			try
			{
				var fileName = $"{label}.json";
				var jsonFilePath = groupLabel != null ? Path.Join(_basePath, groupLabel, fileName) : Path.Join(_basePath, fileName);
				_logger?.LogDebug($"loading {groupLabel}.{label}...");

				var result = JsonSerializer.Deserialize<TEntity>(await File.ReadAllTextAsync(jsonFilePath, Encoding.UTF8));
				_logger?.LogDebug($"{groupLabel}.{label} loaded successfully");

				return result;
			}
			catch (Exception e)
			{
				_logger?.LogDebug($"loading of {groupLabel}.{label} failed with exception {e}");

				return default;
			}
		}
	}
}
