using System;
using System.Collections.Generic;


namespace R136.Entities.General
{
	public class TypedTextsMap<TTextKey> where TTextKey : struct
	{
		private readonly Dictionary<string, IDictionary<TTextKey, ICollection<string>>> _map = new Dictionary<string, IDictionary<TTextKey, ICollection<string>>>();

		public void LoadInitializers(ICollection<Initializer> initializers)
		{
			foreach (var initializer in initializers)
			{
				if (initializer.Texts != null && initializer.Texts.Length > 0)
				{
					_map.TryGetValue(initializer.TypeName, out IDictionary<TTextKey, ICollection<string>>? innerMap);
					if (innerMap == null)
					{
						innerMap = new Dictionary<TTextKey, ICollection<string>>();
						_map[initializer.TypeName] = innerMap;
					}

					innerMap[initializer.ID] = initializer.Texts;
				}
			}
		}

		public IDictionary<TTextKey, ICollection<string>>? this[object o]
		{
			get
			{
				_map.TryGetValue(o.GetType().Name, out var textMap);

				return textMap;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException();

				this[o.GetType()] = value;
			}
		}

		public IDictionary<TTextKey, ICollection<string>> this[Type t]
		{
			set
			{
				if (_map.ContainsKey(t.Name))
					throw new InvalidOperationException($"Text mappings already set for type {t}");

				_map[t.Name] = value;
			}
		}

		public ICollection<string>? this[object o, TTextKey id]
		{
			get
			{
				var textMap = this[o];

				if (textMap == null)
					return null;

				textMap.TryGetValue(id, out var text);

				return text;
			}
			set
			{
				if (value == null)
					throw new ArgumentNullException();

				var textMap = this[o];

				if (textMap == null)
				{
					textMap = new Dictionary<TTextKey, ICollection<string>>();
					this[o] = textMap;
				}

				textMap[id] = value;
			}
		}

		public class Initializer
		{
			public string TypeName { get; set; } = "";
			public TTextKey ID { get; set; }
			public string[]? Texts { get; set; }
		}
	}
}
