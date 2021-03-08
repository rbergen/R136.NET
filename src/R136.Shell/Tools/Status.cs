using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R136.Core;
using R136.Interfaces;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace R136.Shell.Tools
{
	class Status
	{
		private static string? _filename = null;
		private static ILogger<Status>? _logger = null;

		private IServiceProvider? _services;

		public ContinuationStatus? ContinuationStatus { get; set; }
		public InputSpecs? InputSpecs { get; set; }
		public Engine.Snapshot? EngineSnapshot { get; set; }
		public string? Language { get; set; }
		public string[]? Texts { get; set; }
		public bool Pausing { get; set; }

		[JsonIgnore]
		public bool IsLoaded { get; private set; } = false;

		public static Status? Load(IServiceProvider? services)
		{
			try
			{
				var result = JsonSerializer.Deserialize<Status>(File.ReadAllText(GetFilename(services), Encoding.UTF8));

				if (result != null)
				{
					result._services = services;
					result.IsLoaded = true;
				}

				return result;
			}
			catch (Exception e)
			{
				GetLogger(services)?.LogDebug($"Error while reading JSON file: {e}");
				return null;
			}
		}

		public void Remove()
		{
			try
			{
				File.Delete(GetFilename(_services));
			}
			catch (Exception e)
			{
				GetLogger(_services)?.LogDebug($"Error while deleting JSON file: {e}");
			}
		}

		public void Save()
		{
			try
			{
				File.WriteAllText(GetFilename(_services), JsonSerializer.Serialize(this), Encoding.UTF8);
			}
			catch (Exception e)
			{
				GetLogger(_services)?.LogDebug($"Error while writing JSON file: {e}");
			}
		}

		private static string GetFilename(IServiceProvider? services)
		{
			if (_filename == null)
				_filename = services?.GetService<IConfiguration>()?[Constants.StatusFilename];

			return _filename ?? "r136.status";
		}

		private static ILogger<Status>? GetLogger(IServiceProvider? services)
		{
			if (_logger == null)
				_logger = services?.GetService<ILogger<Status>>();

			return _logger;
		}
	}
}
