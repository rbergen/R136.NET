using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace R136.Interfaces
{
	public class ContinuationStatus : ISnapshot
	{
		public string? Key { get; set; }
		public string[]? Texts { get; set; }
		public int[]? Numbers { get; set; }
		public ContinuationStatus? InnerStatus { get; set; }

		[JsonIgnore]
		public int? Number
		{
			get => Numbers != null && Numbers.Length > 0 ? Numbers[0] : null;

			set
			{
				if (value == null)
				{
					Numbers = null;
					return;
				}

				if (Numbers == null || Numbers.Length == 0)
					Numbers = new int[1];

				Numbers[0] = value.Value;
			}
		}

		public void AddBytesTo(List<byte> bytes)
		{
			Key.AddTextBytesTo(bytes);
			Texts.AddTextsBytesTo(bytes);
			Numbers.AddIntsBytesTo(bytes);
			InnerStatus.AddSnapshotBytesTo(bytes);
		}

		public int? LoadBytes(ReadOnlyMemory<byte> bytes)
		{
			int totalBytesRead = 0;
			bool abort = false;

			Key = bytes.ToText().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			Texts = bytes.ToTextArray().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			Numbers = bytes.ToIntArray().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
			if (abort) return null;

			int? bytesRead;
			(InnerStatus, bytesRead) = bytes.ToNullable<ContinuationStatus>();

			return bytesRead != null ? totalBytesRead + bytesRead.Value : null;
		}
	}

	public interface IContinuable
	{
		Result Continue(ContinuationStatus status, string input);
	}
}
