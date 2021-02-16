namespace R136.Entities.General
{
	public class ContinuationStatus
	{
		public IContinuable Continuable { get; }
		public object Data { get; }

		public ContinuationStatus(IContinuable continuable, object data) => (Continuable, Data) = (continuable, data);
	}

	public interface IContinuable
	{
		public Result Continue(object statusData, string input);
	}
}
