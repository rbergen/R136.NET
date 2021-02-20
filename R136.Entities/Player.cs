using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace R136.Entities
{
	public class Player
	{
		private int? _lifePoints;
		private int? _lifePointsFromConfig;

		public Room CurrentRoom { get; set; }

		public IReadOnlyList<Item> Inventory => _inventory;

		private readonly List<Item> _inventory;

		public Player(Room startRoom)
			=> (_lifePoints, _lifePointsFromConfig, CurrentRoom, _inventory) = (null, null, startRoom, new List<Item>());

		public int LifePoints
		{
			get
			{
				if (_lifePointsFromConfig != Facilities.Configuration.LifePoints)
					_lifePoints = _lifePointsFromConfig = Facilities.Configuration.LifePoints;

				return _lifePoints!.Value;
			}

			private set
			{
				_lifePoints = value;
			}
		}

		private ICollection<string>? GetNamedTexts(TextID id, Item item) => Facilities.TextsMap[this, (int)id].ReplaceInAll("{item}", item.Name);

		public Result AddToInventory(Item item)
		{
			if (Facilities.Configuration.MaxInventory != null && _inventory.Where(item => !item.IsWearable).ToArray().Length == Facilities.Configuration.MaxInventory)
				return Result.Failure(GetNamedTexts(TextID.InventoryFull, item));

			_inventory.Add(item);

			return Result.Success(GetNamedTexts(item.IsWearable ? TextID.StartedWearing : TextID.AddedToInventory, item));
		}

		public Item? FindInInventory(ItemID id) 
			=> _inventory.FirstOrDefault(item => item.ID == id);

		public bool RemoveFromInventory(Item item)
			=> _inventory.Remove(item);

		public bool RemoveFromInventory(ItemID id)
			=> _inventory.RemoveAll(item => item.ID == id) > 0;

		public (Item? item, FindResult result) FindInInventory(string s)
			=> _inventory.FindItemByName(s);

		public void DecreaseHealth() 
			=> DecreaseHealth(HealthImpact.Normal);

		public void DecreaseHealth(HealthImpact impact)
		{
			if (Facilities.Configuration.Immortal)
				return;

			LifePoints -= (int)impact;

			if (LifePoints < 0)
				LifePoints = 0;
		}

		public void RestoreHealth()
		{
			if (_lifePointsFromConfig != Facilities.Configuration.LifePoints)
			{
				_lifePointsFromConfig = Facilities.Configuration.LifePoints;
			}

			_lifePoints = _lifePointsFromConfig;
		}

		private enum TextID
		{
			InventoryFull,
			AddedToInventory,
			StartedWearing
		}
	}
}
