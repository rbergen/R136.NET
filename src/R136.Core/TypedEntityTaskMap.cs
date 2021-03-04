using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R136.Core
{
	public class TypedEntityTaskCollection
	{
		private readonly Dictionary<Type, ValueType> _map = new();

		public void Add<TValue>(ValueTask<TValue> value)
			=> _map[typeof(TValue)] = value;

		public void Add<TValue>(Task<TValue> value)
			=> _map[typeof(TValue)] = new ValueTask<TValue>(value);

		public void Add<TValue>(TValue value)
			=> _map[typeof(TValue)] = new ValueTask<TValue>(value);

		public ValueTask<TValue> Get<TValue>()
			=> (ValueTask<TValue>)_map[typeof(TValue)];
	}
}
