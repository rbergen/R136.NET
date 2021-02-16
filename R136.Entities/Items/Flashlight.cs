using R136.Entities.General;
using R136.Entities.Global;
using System.Collections.Generic;
using System.Linq;

namespace R136.Entities.Items
{
	public class Flashlight : Item, ICompound<Item>
	{
		public bool IsOn { get; private set; }

		public int? LampPoints { get; private set; }
		public ICollection<Item> Components { get; private set; }
		public ICollection<string>? CombineTexts
		{
			get => Facilities.ItemTextsMap[this, TextType.Combine];
		}

#pragma warning disable IDE0060 // Remove unused parameter
		public static Flashlight FromInitializer(Initializer initializer, IDictionary<AnimateID, Animate> animates, IDictionary<ItemID, Item> items)
			=> new Flashlight(initializer.ID, initializer.Name, initializer.Description, initializer.StartRoom,
				initializer.Wearable, !initializer.BlockPutdown, items, initializer.Components!);
#pragma warning restore IDE0060 // Remove unused parameter

		public Flashlight(
			ItemID id,
			string name,
			string description,
			RoomID startRoom,
			bool isWearable,
			bool isPutdownAllowed,
			IDictionary<ItemID, Item> items,
			ICollection<ItemID> components
		) : base(id, name, description, startRoom, isWearable, isPutdownAllowed)
			=> (IsOn, LampPoints, Components)
			= (false, Facilities.Configuration.LampPoints, components.Select(itemID => itemID == id ? this : items[itemID]).ToArray());


		private ICollection<string>? GetTexts(TextID id) => Facilities.TextsMap[this, (int)id];

		public override Result Use()
		{
			if (IsOn)
			{
				IsOn = false;

				var isDark = StatusManager?.IsDark ?? true;

				return new Result(ResultCode.Success, GetTexts(isDark ? TextID.LightOffInDark : TextID.LightOff));
			}

			if (LampPoints == null || LampPoints-- > 0)
			{
				IsOn = true;
				return new Result(ResultCode.Success, GetTexts(TextID.LightOn));
			}

			return new Result(ResultCode.Success, GetTexts(TextID.NeedBatteries));
		}

		public Result Combine(Item first, Item second)
		{
			if (!Components.Contains(first) || !Components.Contains(second) || first == second)
				return Result.Failure();

			if (first != this)
				StatusManager?.RemoveFromPossession(first.ID);
			if (second != this)
				StatusManager?.RemoveFromPossession(second.ID);

			return new Result(ResultCode.Success, CombineTexts);
		}

		private enum TextID
		{
			LightOff,
			LightOffInDark,
			LightOn,
			NeedBatteries
		}
	}
}
