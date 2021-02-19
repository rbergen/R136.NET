namespace R136.Interfaces
{
	public class ContinuationStatus
	{
		public IContinuable Continuable { get; }
		public object Data { get; }

		public ContinuationStatus(IContinuable continuable, object data) => (Continuable, Data) = (continuable, data);
	}

	public interface IContinuable
	{
		Result Continue(object statusData, string input);
	}
}
