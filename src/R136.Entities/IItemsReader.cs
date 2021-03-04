using R136.Interfaces;
using System.Collections.Generic;

namespace R136.Entities
{
	public interface IItemsReader
	{
		IReadOnlyDictionary<ItemID, Item>? Items { set; }
	}
}
