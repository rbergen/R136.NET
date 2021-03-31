using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace R136.Entities.General
{
	public static class ExtensionMethods
	{
		public static void AddRangeIfNotNull<T>(this List<T> list, IEnumerable<T>? collection)
		{
			if (collection != null)
				list.AddRange(collection);
		}

		public static StringValues ReplaceInAll(this StringValues collection, string from, string to)
			=> collection.Select(s => s.Replace(from, to)).ToArray();

		public static (Item? item, FindResult result) FindItemByName(this List<Item> list, string s)
		{
			var foundItems = list.FindAll(item => item.Name.Contains(s));

			FindResult result = foundItems.Count switch
			{
				0 => FindResult.NotFound,
				1 => FindResult.Found,
				_ => FindResult.Ambiguous
			};

			return ((result == FindResult.Found ? foundItems[0] : null), result);
		}

		public static (int index, string? found) IndexOfAny(this string subject, StringValues substrings)
		{
			if (substrings == StringValues.Empty)
				return (-1, null);

			foreach (var substring in substrings)
			{
				int index = subject.IndexOf(substring);
				if (index >= 0)
					return (index, substring);
			}

			return (-1, null);
		}

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
