using R136.Entities.General;
using R136.Entities.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities
{
	public class Player
	{
		public int LifePoints { get; private set; }

		private List<Item> _inventory;

		public Player()
			=> (LifePoints, _inventory) = (Facilities.Configuration.LifePoints, new List<Item>());

		private ICollection<string>? GetNamedTexts(TextID id, Item item) => Facilities.TextsMap[this, (int)id].ReplaceInAll("{item}", item.Name);

		public Result AddToInventory(Item item)
		{
			if (Facilities.Configuration.MaxInventory != null && _inventory.Count == Facilities.Configuration.MaxInventory)
				return Result.Failure(GetNamedTexts(TextID.InventoryFull, item));

			_inventory.Add(item);

			return Result.Success(GetNamedTexts(TextID.AddedToInventory, item));
		}

		public bool RemoveFromInventory(Item item)
		{
			return _inventory.Remove(item);
		}

		public bool RemoveFromInventory(ItemID id)
		{
			return _inventory.RemoveAll(item => item.ID == id) > 0;
		}

		public Item? FindInInventory(string s, out FindResult result)
			=> _inventory.FindItemByName(s, out result);

		public void DecreaseHealth() => DecreaseHealth(HealthImpact.Normal);

		public void DecreaseHealth(HealthImpact impact)
		{
			LifePoints -= (int)impact;

			if (LifePoints < 0)
				LifePoints = 0;
		}

		private enum TextID
		{
			InventoryFull,
			AddedToInventory
		}
	}

	public enum HealthImpact
	{
		Normal = 1,
		Severe = 4
	}
}
