using System.Text.Json.Serialization;

namespace R136.Interfaces
{
	public class InputSpecs
	{
		public int MaxLength { get; set; }
		public string? Permitted { get; set; }
		public bool IsLowerCase { get; set; }

		[JsonConstructor]
		public InputSpecs(int maxLength, string? permitted, bool isLowerCase)
			=> (MaxLength, Permitted, IsLowerCase) = (maxLength, permitted, isLowerCase);

		public InputSpecs(int maxLength) : this(maxLength, null, false) { }
	}
}
