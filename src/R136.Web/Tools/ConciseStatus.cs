using R136.Core;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

		public void AddBytes(List<byte> bytes)
		{
			bytes.AddRange(Watermark);
			IsPaused.AddByte(bytes);
			EngineSnapshot.AddSnapshotBytes(bytes);
			MarkupContentLog.AddSnapshotBytes(bytes);
			ContinuationStatus.AddSnapshotBytes(bytes);
			InputSpecs.AddSnapshotBytes(bytes);
		}

		public int? LoadBytes(ReadOnlyMemory<byte> bytes)
		{
			if (bytes.Length <= BaseBytesSize || !Enumerable.SequenceEqual(bytes[0..Watermark.Length].ToArray(), Watermark))
				return null;

			IsPaused = bytes.Span[0].ToBool();

			int totalBytesRead = BaseBytesSize;
			bytes = bytes[totalBytesRead..];
			bool abort = false;

			EngineSnapshot = bytes.ToNullable<Engine.Snapshot>().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			MarkupContentLog = bytes.ToNullable<MarkupContentLog.Snapshot>().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			ContinuationStatus = bytes.ToNullable<ContinuationStatus>().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			int? bytesRead;
			(InputSpecs, bytesRead) = bytes.ToNullable<InputSpecs>();

			return bytesRead != null ? totalBytesRead + bytesRead.Value : null;
		}
	}
}

#nullable restore