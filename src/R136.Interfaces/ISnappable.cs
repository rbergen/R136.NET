using System;

namespace R136.Interfaces
{
	public interface ISnappable<TSnapshot>
		where TSnapshot : class, ISnapshot, new()
	{
		TSnapshot TakeSnapshot(TSnapshot? snapshot = null);

		bool RestoreSnapshot(TSnapshot snapshot);
	}
}

public interface ISnapshot
{
	byte[] GetBinary();
	
	int? SetBinary(Span<byte> value);

}
