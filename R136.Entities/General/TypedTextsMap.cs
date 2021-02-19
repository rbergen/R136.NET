using System;
using System.Collections.Generic;


namespace R136.Entities.General
{
	public class TypedTextsMap<TTextKey> : KeyedTextsMap<string, TTextKey> where TTextKey : struct
	{
		public void LoadInitializers(ICollection<Initializer>? initializers)
		{
			if (initializers == null)
				return;

			foreach (var initializer in initializers)
			{
				if (initializer.Texts != null && initializer.Texts.Length > 0)
					this[initializer.Type, initializer.ID] = initializer.Texts;
			}
		}

		public IDictionary<TTextKey, ICollection<string>>? this[object o]
		{
			get => base[o.GetType().Name];
			set
			{
				if (value == null)
					throw new ArgumentNullException(nameof(value));

				base[o.GetType().Name] = value;
			}
		}

		public IDictionary<TTextKey, ICollection<string>> this[Type t]
		{
			set
			{
				base[t.Name] = value;
			}
		}

		public ICollection<string>? this[object o, TTextKey id]
		{
			get => base[o.GetType().Name, id];
			set
			{
				base[o.GetType().Name, id] = value;
			}
		}

		public class Initializer
		{
			public string Type { get; set; } = "";
			public TTextKey ID { get; set; }
			public string[]? Texts { get; set; }
		}
	}
}
