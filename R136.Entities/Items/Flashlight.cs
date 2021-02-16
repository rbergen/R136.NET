using R136.Entities.General;
using R136.Entities.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Items
{
	public class Flashlight : Item, ICompound<Item>
	{
		public ICollection<Item> Components { get; private set;  }
		public ICollection<string>? CombineTexts { get; }

#pragma warning disable IDE0060 // Remove unused parameter
		public static Flashlight FromInitializer(Initializer initializer, IDictionary<AnimateID, Animate> animates, IDictionary<ItemID, Item> items)
			=> new Flashlight(initializer.ID, initializer.Name, initializer.Description, initializer.StartRoom, 
				initializer.Components!, items, initializer.CombineTexts, initializer.Wearable, !initializer.BlockPutdown);
#pragma warning restore IDE0060 // Remove unused parameter

		public Flashlight(
			ItemID id,
			string name,
			string description,
			RoomID startRoom,
			ICollection<ItemID> componentIDs,
			IDictionary<ItemID, Item> items,
			ICollection<string>? combineTexts,
			bool isWearable,
			bool isPutdownAllowed
		) : base(id, name, description, startRoom, null, isWearable, isPutdownAllowed)
		{
			CombineTexts = combineTexts;
			Components = componentIDs.Select(itemID => itemID == id ? this : items[itemID]).ToArray();
		}
			

		public override Result Use()
		{
			if (StatusManager != null && StatusManager.LifePoints == Facilities.Configuration.LifePoints)
				return new Result(ResultCode.Success, Facilities.TextsMap[this, (int)TextID.FullHealth]);

			StatusManager?.RestoreHealth();
			return base.Use();
		}

		public Result Combine(Item first, Item second)
		{
			if (!Components.Contains(first) || !Components.Contains(second) || first == second)
				return Result.Failure;

			if (first != this)
				StatusManager?.RemoveFromPossession(first.ID);
			if (second != this)
				StatusManager?.RemoveFromPossession(second.ID);

			return new Result(ResultCode.Success, CombineTexts);
		}
		private enum TextID
		{
			LightOn
		}
	}
}
