using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.General
{
	public class KeyedTextsMap<TDictKey, TTextKey>
			where TDictKey : notnull
			where TTextKey : struct
	{
		private readonly Dictionary<TDictKey, IDictionary<TTextKey, ICollection<string>>> _map 
			= new Dictionary<TDictKey, IDictionary<TTextKey, ICollection<string>>>();

		public IDictionary<TTextKey, ICollection<string>>? this[TDictKey key]
		{
			get
			{
				_map.TryGetValue(key, out var textMap);

				return textMap;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				_map[key] = value;
			}
		}

		public ICollection<string>? this[TDictKey key, TTextKey id]
		{
			get
			{
				var textMap = this[key];

				if (textMap == null)
					return null;

				textMap.TryGetValue(id, out var text);

				return text;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				var textMap = this[key];

				if (textMap == null)
				{
					textMap = new Dictionary<TTextKey, ICollection<string>>();
					this[key] = textMap;
				}

				textMap[id] = value;
			}
		}
	}
}
