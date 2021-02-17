using System.Collections.Generic;
using System.Linq;

namespace R136.Entities.General
{
	public static class CollectionHelper
	{
		public static void AddRangeIfNotNull<T>(this List<T> list, IEnumerable<T>? collection)
		{
			if (collection != null)
				list.AddRange(collection);
		}

		public static ICollection<string>? ReplaceInAll(this ICollection<string>? collection, string from, string to)
			=> collection == null ? null : collection.Select(s => s.Replace(from, to)).ToArray();
	
		public static Item? FindItemByName(this List<Item> list, string s, out FindResult result)
		{
			var foundItems = list.FindAll(item => item.Name.Contains(s));

			result = foundItems.Count switch
			{
				0 => FindResult.NotFound,
				1 => FindResult.Found,
				_ => FindResult.Ambiguous
			};

			return result == FindResult.Found ? foundItems[0] : null;
		}
	}
}
