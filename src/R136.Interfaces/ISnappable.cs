using System;
using System.Collections.Generic;

namespace R136.Interfaces
{
	public interface ISnappable<TSnapshot>
		where TSnapshot : class, ISnapshot, new()
	{
		TSnapshot TakeSnapshot(TSnapshot? snapshot = null);

		bool RestoreSnapshot(TSnapshot snapshot);
	}

	public interface ISnapshot
	{
		void AddBytesTo(List<byte> bytes);

		int? LoadBytes(ReadOnlyMemory<byte> bytes);
	}
}