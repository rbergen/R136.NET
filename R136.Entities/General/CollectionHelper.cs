using System.Collections.Generic;

namespace R136.Entities.General
{
	public static class CollectionHelper
	{
		public static void AddRangeIfNotNull<TValue>(this List<TValue> list, IEnumerable<TValue>? collection)
		{
			if (collection != null)
				list.AddRange(collection);
		}
	}
}
