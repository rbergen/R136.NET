using System;
using System.Collections.Generic;
using System.Linq;

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
				var textMap = this[key];

				if (value == null || value.Count == 0)
				{
					if (textMap != null)
						textMap.Remove(id);
					
					return;
				}

				if (textMap == null)
				{
					textMap = new Dictionary<TTextKey, ICollection<string>>();
					this[key] = textMap;
				}

				textMap[id] = value;
			}
		}

		public void LoadInitializer(TDictKey key, IInitializer initializer)
			=> this[key, initializer.ID] = initializer.Texts;

		protected void Clear()
			=> _map.Clear();

		public int TextsMapCount => _map.Count;
		public int TextValueCount => _map.Aggregate(0, (count, pair) => count += pair.Value.Count, total => total);

		public interface IInitializer
		{
			TTextKey ID { get; }

			string[]? Texts { get; }
		}
	}
}
