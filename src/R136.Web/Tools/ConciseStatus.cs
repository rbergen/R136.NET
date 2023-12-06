using R136.Core;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace R136.Web.Tools
{
	public class ConciseStatus : ISnapshot
	{
		private const int BaseBytesSize = 13;
		private readonly static byte[] Watermark = { 18, 9, 16, 24, 5, 2, 19, 20, 1, 20, 21, 19 };

		public Engine.Snapshot? EngineSnapshot { get; set; }
		public MarkupContentLog.Snapshot? MarkupContentLog { get; set; }
		public ContinuationStatus? ContinuationStatus { get; set; }
		public InputSpecs? InputSpecs { get; set; }
		public bool IsPaused { get; set; }
		public bool IsLoaded { get; set; }
		public string? Language { get; set; }

		public void AddBytesTo(List<byte> bytes)
		{
			bytes.AddRange(Watermark);
			IsPaused.AddByteTo(bytes);
			EngineSnapshot.AddSnapshotBytesTo(bytes);
			MarkupContentLog.AddSnapshotBytesTo(bytes);
			ContinuationStatus.AddSnapshotBytesTo(bytes);
			Language.AddTextBytesTo(bytes);
			InputSpecs.AddSnapshotBytesTo(bytes);
			true.AddByteTo(bytes);
		}

		public int? LoadBytes(ReadOnlyMemory<byte> bytes)
		{
			if (bytes.Length <= BaseBytesSize || !Enumerable.SequenceEqual(bytes[0..Watermark.Length].ToArray(), Watermark))
				return null;

			IsPaused = bytes.Span[Watermark.Length].ToBool();

			int totalBytesRead = BaseBytesSize;
			bytes = bytes[totalBytesRead..];
			bool abort = false;

			EngineSnapshot = bytes.ToNullable<Engine.Snapshot>().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			MarkupContentLog = bytes.ToNullable<MarkupContentLog.Snapshot>().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			ContinuationStatus = bytes.ToNullable<ContinuationStatus>().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			Language = bytes.ToText().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			InputSpecs = bytes.ToNullable<InputSpecs>().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			IsLoaded = bytes.Span[0].ToBool();

			return totalBytesRead + 1;
		}
	}
}

#nullable restore