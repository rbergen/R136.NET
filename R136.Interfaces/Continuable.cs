using System.Text.Json.Serialization;

namespace R136.Interfaces
{
	public class ContinuationStatus
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
	}

	public interface IContinuable
	{
		Result Continue(ContinuationStatus status, string input);
	}
}
