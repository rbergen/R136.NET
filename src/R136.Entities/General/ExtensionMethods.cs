using Microsoft.Extensions.Primitives;
using System;
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
			=> collection.Select(s => (s ?? string.Empty).Replace(from, to)).ToArray();

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
				if (substring == null) 
					continue;
				int index = subject.IndexOf(substring);
				if (index >= 0)
					return (index, substring);
			}

			return (-1, null);
		}

		public static string ReplacePlaceholders(this string source, IReadOnlyDictionary<string, object> valueMap)
		{
			Regex regex = new("{(?<placeholder>[a-z_][a-z0-9_]*?)}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

			return regex.Replace(source, ValueMapper);

			string ValueMapper(Match match)
			{
				string key = match.Groups["placeholder"].Value;

				return valueMap.TryGetValue(key, out object? value)
					? value.ToString() ?? string.Empty
					: key;
			}
		}

		public static TValue Get<TValue, TIndex>(this IReadOnlyList<TValue> list, TIndex index) where TIndex : Enum
			=> list[Convert.ToInt32(index)];

		public static void Set<TValue, TIndex>(this IList<TValue> list, TIndex index, TValue value) where TIndex : Enum
			=> list[Convert.ToInt32(index)] = value;

		public static TValue Get<TValue, TIndex1, TIndex2>(this TValue[][] array, TIndex1 index1, TIndex2 index2) 
			where TIndex1 : Enum
			where TIndex2 : Enum
			=> array[Convert.ToInt32(index1)][Convert.ToInt32(index2)];

		public static void Set<TValue, TIndex1, TIndex2>(this TValue[][] array, TIndex1 index1, TIndex2 index2, TValue value) 
			where TIndex1 : Enum
			where TIndex2 : Enum
			=> array[Convert.ToInt32(index1)][Convert.ToInt32(index2)] = value;

		public static StringValues Get<TDictKey, TIndex>(this KeyedTextsMap<TDictKey, int> map, TDictKey key, TIndex index)
			where TDictKey : notnull
			where TIndex : Enum
			=> map[key, Convert.ToInt32(index)];

		public static StringValues Get<TIndex>(this TypedTextsMap<int> map, object key, TIndex index)
			where TIndex : Enum
			=> map[key, Convert.ToInt32(index)];
	}
}
