using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace R136.Shell.Tools
{
	public static class ExtensionMethods
	{
		public static string ToPlainText(this StringValues texts)
		{
			if (texts == StringValues.Empty)
				return string.Empty;

			return string.Join('\n', texts.Select(t => t.Replace("\\", string.Empty)));
		}

		public static void AddIfNotEmpty(this IList<StringValues> list, StringValues value)
		{
			if (value != StringValues.Empty)
				list.Add(value);
		}

		public static bool EqualsAny(this string s, ICollection<string> options, StringComparison comparison = StringComparison.Ordinal)
			=> options.Any(o => s.Equals(o, comparison));

		public static string ReplacePlaceholders(this string source, IReadOnlyDictionary<string, object> valueMap)
		{
			var regex = new Regex("{(?<placeholder>[a-z_][a-z0-9_]*?)}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

			return regex.Replace(source, ValueMapper);

			string ValueMapper(Match match)
			{
				string key = match.Groups["placeholder"].Value;

				return valueMap.TryGetValue(key, out object? value)
					? value.ToString() ?? string.Empty
					: key;
			}
		}
	}
}
