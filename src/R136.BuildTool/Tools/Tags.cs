namespace R136.BuildTool.Tools
{
	static class Tags
	{
		public const string Info = "  ";
		public const string Success = "++";
		public const string Warning = "--";
		public const string Error = "!!";

		public static bool IsError(string s)
			=> s == Error;

		public static bool IsWarning(string s)
			=> s == Warning;
	}
}
