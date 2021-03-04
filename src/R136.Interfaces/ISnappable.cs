namespace R136.Interfaces
{
	public interface ISnappable<TSnapshot>
		where TSnapshot : class
	{
		TSnapshot TakeSnapshot(TSnapshot? snapshot = null);

		bool RestoreSnapshot(TSnapshot snapshot);
	}
}
