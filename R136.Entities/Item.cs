using R136.Entities.General;
using R136.Entities.Global;
using R136.Entities.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace R136.Entities
{
	public class Item : EntityBase
	{
		public ItemID ID { get; private set; }
		public string Name { get; }
		public string Description { get; }
		public RoomID CurrentRoom { get; set; }
		public bool IsPutdownAllowed { get; }
		public bool IsWearable { get; }


		public static Item FromInitializer(Initializer initializer)
			=> new Item(initializer.ID, initializer.Name, initializer.Description, initializer.StartRoom, initializer.Wearable, !initializer.BlockPutdown);

		public static IDictionary<ItemID, Item> FromInitializers(ICollection<Initializer> initializers, IDictionary<AnimateID, Animate> animates)
		{
			Dictionary<ItemID, Item> items = new Dictionary<ItemID, Item>(initializers.Count);

			var compoundInitializers = new List<Initializer>();
			var typedInitializers = new Dictionary<Initializer, Type>();

			foreach (var initializer in initializers)
			{
				Type? itemType = ToType(initializer.ID);

				if (itemType != null)
					typedInitializers[initializer] = itemType;

				else if (initializer.UsableOn != null && initializer.UsableOn.Length > 0)
				{
					if (initializer.Components != null && initializer.Components.Length == 2)
						compoundInitializers.Add(initializer);
					else
						items[initializer.ID] = RegisterTexts(UsableItem.FromInitializer(initializer, animates), initializer);
				}
				else
					items[initializer.ID] = RegisterTexts(FromInitializer(initializer), initializer);
			}

			foreach (var initializer in compoundInitializers)
				items[initializer.ID] = RegisterTexts(CompoundItem.FromInitializer(initializer, animates, items), initializer);

			foreach (var initializerType in typedInitializers)
			{
				var initializer = initializerType.Key;

				var method = initializerType.Value.GetMethod(nameof(FromInitializer),
					new Type[] { typeof(Initializer), typeof(IDictionary<AnimateID, Animate>), typeof(IDictionary<ItemID, Item>) });

				if (method != null && method.IsStatic && method.ReturnType.IsAssignableTo(typeof(Item)))
				{
					Item? item = (Item?)method.Invoke(null, new object[] { initializer, animates, items });

					if (item != null)
						items[initializer.ID] = RegisterTexts(item, initializer);
				}
			}

			return items;
		}

		private static Item RegisterTexts(Item item, Initializer initializer)
		{
			if (initializer.UseTexts != null)
				Facilities.ItemTextsMap[item, TextType.Use] = initializer.UseTexts;

			if (initializer.CombineTexts != null)
				Facilities.ItemTextsMap[item, TextType.Combine] = initializer.UseTexts;

			return item;
		}

		public Item(ItemID id, string name, string description, RoomID startRoom, bool isWearable, bool isPutdownAllowed)
			=> (ID, Name, Description, CurrentRoom, IsWearable, IsPutdownAllowed)
			= (id, name, description, startRoom, isWearable, isPutdownAllowed);

		public virtual Result Use() => Result.Success(UseTexts);

		public ICollection<string>? UseTexts
		{
			get => Facilities.ItemTextsMap[this, TextType.Use];
		}

		public class Initializer
		{
			public ItemID ID { get; set; }
			public string Name { get; set; } = "";
			public string Description { get; set; } = "";
			public RoomID StartRoom { get; set; }

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			public ICollection<string>? UseTexts { get; set; }

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
			public bool BlockPutdown { get; set; }

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
			public bool Wearable { get; set; }

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			public ItemID[]? Components { get; set; }

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			public ICollection<string>? CombineTexts { get; set; }

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			public AnimateID[]? UsableOn { get; set; } = null;

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
			public bool KeepAfterUse { get; set; }
		}

		public enum TextType
		{
			Use,
			Combine
		}

		private static Type? ToType(ItemID id) => id switch
		{
			ItemID.TNT => typeof(Tnt),
			_ => null
		};
	}

	public interface IUsable<T>
	{
		public ICollection<T> UsableOn { get; }

		public Result UseOn(T subject);
	}

	public class UsableItem : Item, IUsable<Animate>
	{
		public ICollection<Animate> UsableOn { get; }
		public bool KeepAfterUse { get; }

		public static UsableItem FromInitializer(Initializer initializer, IDictionary<AnimateID, Animate> animates)
			=> new UsableItem(
				initializer.ID,
				initializer.Name,
				initializer.Description,
				initializer.StartRoom,
				initializer.UsableOn!.Select(animateID => animates[animateID]).ToArray(),
				initializer.Wearable,
				!initializer.BlockPutdown,
				initializer.KeepAfterUse
			);

		public UsableItem(
			ItemID id,
			string name,
			string description,
			RoomID startRoom,
			ICollection<Animate> usableOn,
			bool isWearable,
			bool isPutdownAllowed,
			bool keepAfterUse
		) : base(id, name, description, startRoom, isWearable, isPutdownAllowed)
			=> (UsableOn, KeepAfterUse) = (usableOn, keepAfterUse);

		public virtual Result UseOn(Animate animate)
		{
			if (!UsableOn.Contains(animate))
				return Use();

			if (animate.Used(ID) && !KeepAfterUse)
				StatusManager?.RemoveFromPossession(ID);

			return Result.Success();
		}
	}

	public interface ICompound<T>
	{
		public ICollection<T> Components { get; }
		public Result Combine(T first, T second);
	}

	public class CompoundItem : UsableItem, ICompound<Item>
	{
		public ICollection<Item> Components { get; }
		public ICollection<string>? CombineTexts
		{
			get => Facilities.ItemTextsMap[this, TextType.Combine];
		}

		public static CompoundItem FromInitializer(Initializer initializer, IDictionary<AnimateID, Animate> animates, IDictionary<ItemID, Item> items)
			=> new CompoundItem(
				initializer.ID,
				initializer.Name,
				initializer.Description,
				initializer.StartRoom,
				initializer.UsableOn!.Select(animateID => animates[animateID]).ToArray(),
				initializer.Components!.Select(itemID => items[itemID]).ToArray(),
				initializer.Wearable,
				!initializer.BlockPutdown,
				initializer.KeepAfterUse
			);

		public CompoundItem(
			ItemID id,
			string name,
			string description,
			RoomID startRoom,
			ICollection<Animate> usableOn,
			ICollection<Item> components,
			bool isWearable,
			bool isPutdownAllowed,
			bool keepAfterUse
		)
			: base(id, name, description, startRoom, usableOn, isWearable, isPutdownAllowed, keepAfterUse)
			=> Components = components;

		public Result Combine(Item first, Item second)
		{
			if (!Components.Contains(first) || !Components.Contains(second) || first == second)
				return Result.Failure();

			if (first != this)
				StatusManager?.RemoveFromPossession(first.ID);
			if (second != this)
				StatusManager?.RemoveFromPossession(second.ID);

			return Result.Success(CombineTexts);
		}

		private enum TextID
		{
			CantCombineWithItself = 0,
		}
	}

	public enum ItemID
	{
		HoundMeat = 0,
		HeatSuit = 1,
		GreenCrystal = 2,
		Sword = 3,
		Bone = 4,
		Diskette = 5,
		Hashies = 6,
		RedCrystal = 7,
		Nightcap = 8,
		Bomb = 9,
		Flashlight = 10,
		Bandage = 11,
		Flamethrower = 12,
		Cookbook = 13,
		TNT = 14,
		GasCanister = 15,
		PoisonedMeat = 16,
		Ignition = 17,
		Batteries = 18,
		GasMask = 19,
		Paper = 20,
		Pornbook = 21,
		BlueCrystal = 22,
		Cookie = 23,
		GasGrenade = 24
	}

	public enum FindResult
	{
		Found,
		NotFound,
		Ambiguous
	}
}
