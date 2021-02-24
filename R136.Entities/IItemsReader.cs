using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities
{
	public interface IItemsReader
	{
		IReadOnlyDictionary<ItemID, Item>? Items { set; }
	}
}
