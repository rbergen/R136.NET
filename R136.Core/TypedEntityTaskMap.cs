using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Core
{
	public class TypedEntityCollection
	{
		private Dictionary<Type, object> _map = new Dictionary<Type, object>();

		public void Add<TValue>(TValue value) where TValue : notnull
			=> _map[typeof(TValue)] = value;

		public TValue Get<TValue>()
			=> (TValue)_map[typeof(TValue)];
	}
}
