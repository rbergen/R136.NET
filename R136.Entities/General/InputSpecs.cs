namespace R136.Entities.General
{
	public class InputSpecs
	{
		public int MaxLength { get; set; }
		public string? PermittedCharacters { get; set; }

		public InputSpecs(int maxLength, string? permittedCharacters)
			=> (MaxLength, PermittedCharacters) = (maxLength, permittedCharacters);

		public InputSpecs(int maxLength) : this(maxLength, null) { }
	}
}
