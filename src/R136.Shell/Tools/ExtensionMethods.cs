using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;

namespace R136.Shell.Tools
{
	public static class ExtensionMethods
	{
		public static string ToPlainText(this StringValues texts)
		{
			if (texts == StringValues.Empty)
				return string.Empty;

			return string.Join('\n', texts.Select(t => t?.Replace("\\", string.Empty)));
		}

		public static void AddIfNotEmpty(this IList<StringValues> list, StringValues value)
		{
			if (value != StringValues.Empty)
				list.Add(value);
		}

		public static bool EqualsAny(this string s, ICollection<string> options, StringComparison comparison = StringComparison.Ordinal)
			=> options.Any(o => s.Equals(o, comparison));
	}
}
