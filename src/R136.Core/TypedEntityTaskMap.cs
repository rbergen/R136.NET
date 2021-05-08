using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R136.Core
{
	public class TypedEntityTaskCollection
	{
		private readonly Dictionary<Type, ValueType> map = new();

		public void Add<TValue>(ValueTask<TValue> value)
			=> this.map[typeof(TValue)] = value;

		public void Add<TValue>(Task<TValue> value)
			=> this.map[typeof(TValue)] = new ValueTask<TValue>(value);

		public void Add<TValue>(TValue value)
			=> this.map[typeof(TValue)] = new ValueTask<TValue>(value);

		public ValueTask<TValue> Get<TValue>()
			=> (ValueTask<TValue>)this.map[typeof(TValue)];
	}
}
