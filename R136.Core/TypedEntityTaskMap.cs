using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Core
{
	public class TypedEntityCollection
	{
		private Dictionary<Type, Task> _map = new Dictionary<Type, Task>();

		public void Add<TValue>(Task<TValue> value) where TValue : notnull
			=> _map[typeof(TValue)] = value;

		public TValue Get<TValue>()
			=> ((Task<TValue>)_map[typeof(TValue)]).Result;
	}
}
