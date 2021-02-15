using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using R136.Entities.Utilities;

namespace R136.Entities.Utilities
{
	public class TextsMap
	{
		private readonly Dictionary<string, Dictionary<int, ICollection<string>>> _map = new Dictionary<string, Dictionary<int, ICollection<string>>>();

		public void LoadInitializers(ICollection<Initializer> initializers)
		{
			foreach (var initializer in initializers)
			{
				if (initializer.Texts != null && initializer.Texts.Length > 0)
					_map.GetOrCreate(initializer.TypeName)[initializer.ID] = initializer.Texts;
			}
		}

		public IDictionary<int, ICollection<string>>? this[object o]
		{
			get
			{
				_map.TryGetValue(o.GetType().Name, out var textMap);

				return textMap;
			}
		}

		public ICollection<string>? this[object o, int id]
		{
			get
			{
				var textMap = this[o];

				if (textMap == null)
					return null;

				textMap.TryGetValue(id, out var text);

				return text;
			}
		}

		public class Initializer
		{
			public string TypeName { get; set; } = "";
			public int ID { get; set; }
			public string[]? Texts { get; set; }
		}
	}
}
