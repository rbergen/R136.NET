using R136.Entities.General;
using R136.Entities.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace R136.Entities
{
	public class Item : EntityBase
	{
		public ItemID ID { get; private set; }
		public string Name { get; }
		public string Description { get; }
		public RoomID CurrentRoom { get; set; }
		public ICollection<string>? UseTexts { get; }
		public bool IsPutdownAllowed { get; }
		public bool IsWearable { get; }


		public static Item FromInitializer(Initializer initializer) 
			=> new Item(initializer.ID, initializer.Name, initializer.Description, initializer.StartRoom, initializer.UseTexts, initializer.Wearable, !initializer.BlockPutdown);

		public static IDictionary<ItemID, Item> FromInitializers(ICollection<Initializer> initializers, IDictionary<AnimateID, Animate> animates)
		{
			Dictionary<ItemID, Item> items = new Dictionary<ItemID, Item>(initializers.Count);

			var compoundInitializers = new List<Initializer>();
			var typedInitializers = new Dictionary<Initializer, Type>();

			foreach(var initializer in initializers)
			{
				Type? itemType = ToType(initializer.ID);

				if (itemType != null)
					typedInitializers[initializer] = itemType;

				else if (initializer.Components != null && initializer.Components.Length == 2)
					compoundInitializers.Add(initializer);

				else
				{
					items[initializer.ID] = (initializer.UsableOn != null && initializer.UsableOn.Length > 0)
						? UsableItem.FromInitializer(initializer, animates)
						: FromInitializer(initializer);
				}
			}

			foreach(var initializer in compoundInitializers)
				items[initializer.ID] = CompoundItem.FromInitializer(initializer, items);

			foreach(var initializerType in typedInitializers)
			{
				var initializer = initializerType.Key;

				var method = initializerType.Value.GetMethod(nameof(FromInitializer),
					new Type[] { typeof(Initializer), typeof(IDictionary<ItemID, Item>), typeof(IDictionary<AnimateID, Animate>) });

				if (method != null && method.IsStatic && method.ReturnType.IsAssignableTo(typeof(Item)))
				{
					Item? item = (Item?)method.Invoke(null, new object[] { initializer, items, animates });
					
					if (item != null)
						items[initializer.ID] = item;
				}
			}

			return items;
		}

		public Item(ItemID id, string name, string description, RoomID startRoom, ICollection<string>? useTexts, bool isWearable, bool isPutdownAllowed)
			=> (ID, Name, Description, CurrentRoom, UseTexts, IsWearable, IsPutdownAllowed) 
			= (id, name, description, startRoom, useTexts, isWearable, isPutdownAllowed);

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
			public AnimateID[]? UsableOn { get; set; } = null;
			
			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
			public bool KeepAfterUse { get; set; }
		}

		private static Type? ToType(ItemID id) => id switch {
			_ => null
		};
	}

	public class CompoundItem : Item {
		public ICollection<Item> Components;

		public static CompoundItem FromInitializer(Initializer initializer, IDictionary<ItemID, Item> items)
			=> new CompoundItem(initializer.ID, initializer.Name, initializer.Description, initializer.StartRoom, 
					initializer.Components!.Select(itemID => items[itemID]).ToArray(), initializer.UseTexts, initializer.Wearable, !initializer.BlockPutdown);

		public CompoundItem(ItemID id, string name, string description, RoomID startRoom, ICollection<Item> compoundItems, ICollection<string>? useTexts, bool isWearable, bool isPutdownAllowed)
			: base(id, name, description, startRoom, useTexts, isWearable, isPutdownAllowed) => Components = compoundItems;

		public Result Combine(Item first, Item second)
		{
			if (!Components.Contains(first) || !Components.Contains(second) || first == second)
				return Result.Failure;

			if (first != this)
				StatusManager?.RemoveFromPossession(first.ID);
			if (second != this)
				StatusManager?.RemoveFromPossession(second.ID);

			return Result.Success;
		}

		private enum TextID
		{
			CantCombineWithItself = 0,
		}
	}

	public class UsableItem : Item
	{
		public ICollection<Animate> UsableOn { get; }
		public bool KeepAfterUse { get; }

		public static UsableItem FromInitializer(Initializer initializer, IDictionary<AnimateID, Animate> animates) 
			=> new UsableItem(initializer.ID, initializer.Name, initializer.Description, initializer.StartRoom,
						initializer.UsableOn!.Select(animateID => animates[animateID]).ToArray(), initializer.UseTexts, initializer.Wearable, !initializer.BlockPutdown, initializer.KeepAfterUse);

		public UsableItem(ItemID id, string name, string description, RoomID startRoom, ICollection<Animate> usableOn, ICollection<string>? useTexts, bool isWearable, bool isPutdownAllowed, bool keepAfterUse)
			: base(id, name, description, startRoom, useTexts, isWearable, isPutdownAllowed) 
			=> (UsableOn, KeepAfterUse) = (usableOn, keepAfterUse);

		public virtual Result UseOn(Animate animate)
		{
			if (!UsableOn.Contains(animate))
				return Result.Failure;

			if (animate.Used(ID))
			{
				if (!KeepAfterUse)
					StatusManager?.RemoveFromPossession(ID);

				return Result.Success;
			}

			return Result.Failure;
		}
	}

	public enum ItemID
	{
		HoundMeat			=  0,
		HeatSuit			=  1,
		GreenCrystal	=  2,
		Sword					=  3,
		Bone					=  4,
		Diskette			=  5,
		Hashies				=  6,
		RedCrystal		=  7,
		Nightcap			=  8,
		Bomb					=  9,
		Flashlight		= 10,
		Bandage				= 11,
		Flamethrower	= 12,
		Cookbook			= 13,
		TNT						= 14,
		GasCanister		= 15,
		PoisonedMeat	= 16,
		Ignition			= 17,
		Batteries			= 18,
		GasMask				= 19,
		Paper					= 20,
		Pornbook			= 21,
		BlueCrystal		= 22,
		Cookie				= 23,
		GasGrenade		= 24
	}
}
