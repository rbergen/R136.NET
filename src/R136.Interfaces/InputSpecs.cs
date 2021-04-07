using System;
using System.Collections.Generic;

namespace R136.Interfaces
{
	public class InputSpecs : ISnapshot
	{
		private const int BytesBaseSize = 1;

		public int MaxLength { get; set; }
		public string? Permitted { get; set; }
		public bool IsLowerCase { get; set; }

		public InputSpecs(int maxLength, string? permitted, bool isLowerCase)
			=> (MaxLength, Permitted, IsLowerCase) = (maxLength, permitted, isLowerCase);

		public InputSpecs(int maxLength) : this(maxLength, null, false) { }

		public InputSpecs() { }

		public void AddBytes(List<byte> bytes)
		{
			IsLowerCase.AddByte(bytes);
			MaxLength.AddBytes(bytes);
			Permitted.AddTextBytes(bytes);
		}

		public int? LoadBytes(ReadOnlyMemory<byte> bytes)
		{
			if (bytes.Length <= BytesBaseSize)
				return null;

			IsLowerCase = bytes.Span[0].ToBool();

			bytes = bytes[BytesBaseSize..];

			int? bytesRead;
			int totalBytesRead = BytesBaseSize;

			(MaxLength, bytesRead) = bytes.ToInt();
			if (bytesRead == null) return null;

			bytes = bytes[bytesRead.Value..];
			totalBytesRead += bytesRead.Value;

			(Permitted, bytesRead) = bytes.ToText();

			return bytesRead != null ? bytesRead.Value + totalBytesRead : null;
		}
	}
}
