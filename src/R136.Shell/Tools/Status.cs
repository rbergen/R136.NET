using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using R136.Core;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace R136.Shell.Tools
{
	class Status : ISnapshot
	{
		private const int BytesBaseSize = 10;
		private readonly static byte[] Watermark = { 18, 9, 16, 19, 20, 1, 20, 21, 19 };

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
			Status? result = new();

			try
			{
				if (result.LoadBytes(File.ReadAllBytes(GetFilename(services))) == null)
					result = null;
			}
			catch (Exception e)
			{
				GetLogger(services)?.LogDebug($"Error while loading file as bytes: {e}");
				result = null;
			}

			if (result?.IsLoaded ?? false)
			{
				result._services = services;
				return result;
			}

			try
			{
				result = JsonSerializer.Deserialize<Status>(File.ReadAllText(GetFilename(services), Encoding.UTF8));
			}
			catch (Exception e)
			{
				GetLogger(services)?.LogDebug($"Error while loading file as JSON: {e}");
			}

			if (result != null)
			{
				result._services = services;
				result.IsLoaded = true;
			}

			return result;

		}

		public void Remove()
		{
			try
			{
				File.Delete(GetFilename(_services));
			}
			catch (Exception e)
			{
				GetLogger(_services)?.LogDebug($"Error while deleting file: {e}");
			}
		}

		public void Save()
		{
			List<byte> bytes = new();
			AddBytes(bytes);

			try
			{
				File.WriteAllBytes(GetFilename(_services), bytes.ToArray());
			}
			catch (Exception e)
			{
				GetLogger(_services)?.LogDebug($"Error while writing file as bytes: {e}");
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

		public void AddBytes(List<byte> bytes)
		{
			bytes.AddRange(Watermark);
			Pausing.AddByte(bytes);
			ContinuationStatus.AddSnapshotBytes(bytes);
			InputSpecs.AddSnapshotBytes(bytes);
			EngineSnapshot.AddSnapshotBytes(bytes);
			Language.AddTextBytes(bytes);
			Texts.AddTextsBytes(bytes);
			true.AddByte(bytes);
		}

		public int? LoadBytes(ReadOnlyMemory<byte> bytes)
		{
			if (bytes.Length <= BytesBaseSize || !Enumerable.SequenceEqual(bytes[0..Watermark.Length].ToArray(), Watermark))
				return null;

			Pausing = bytes.Span[Watermark.Length].ToBool();

			bytes = bytes[BytesBaseSize..];
			int totalBytesRead = BytesBaseSize;
			bool abort = false;

			ContinuationStatus = bytes.ToNullable<ContinuationStatus>().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			InputSpecs = bytes.ToNullable<InputSpecs>().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			EngineSnapshot = bytes.ToNullable<Engine.Snapshot>().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			Language = bytes.ToText().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			Texts = bytes.ToTextArray().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			if (bytes.Length < 1)
				return null;

			IsLoaded = bytes.Span[0].ToBool();

			return totalBytesRead + 1;
		}
	}
}
