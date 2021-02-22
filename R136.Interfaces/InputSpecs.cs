using System.Text.Json.Serialization;

namespace R136.Interfaces
{
	public class InputSpecs
	{
		public int MaxLength { get; set; }
		public string? PermittedCharacters { get; set; }
		public bool IsLowerCase { get; set; }

		[JsonConstructor]
		public InputSpecs(int maxLength, string? permittedCharacters, bool isLowerCase)
			=> (MaxLength, PermittedCharacters, IsLowerCase) = (maxLength, permittedCharacters, isLowerCase);

		public InputSpecs(int maxLength) : this(maxLength, null, false) { }
	}
}
