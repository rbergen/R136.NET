using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Utilities
{
	public static class CollectionHelper
	{
		public static TValue GetOrCreate<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
		{
			if (!dictionary.TryGetValue(key, out TValue value))
			{
				value = new TValue();
				dictionary[key] = value;
			}

			return value;
		}
	}
}
