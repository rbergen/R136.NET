using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace R136.Shell.Tools
{
	public class FileSystemJsonEntityReader : IEntityReader
	{
		private readonly string _basePath;

		public FileSystemJsonEntityReader(string basePath)
			=> _basePath = basePath;

		public async Task<TEntity?> ReadEntity<TEntity>(string? groupLabel, string label)
		{
			try
			{
				var fileName = $"{label}.json";
				var jsonFilePath = groupLabel != null ? Path.Combine(_basePath, groupLabel, fileName) : Path.Combine(_basePath, fileName);

				return JsonSerializer.Deserialize<TEntity>(await File.ReadAllTextAsync(jsonFilePath, Encoding.UTF8));
			}
			catch (IOException)
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
