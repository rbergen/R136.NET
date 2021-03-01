using Microsoft.Extensions.Primitives;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Entities.Items;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace R136.Entities
{
	public class Item : EntityBase, ISnappable<Item.Snapshot>
	{
		public ItemID ID { get; }
		public string Name { get; }
		public string Description { get; }
		public RoomID CurrentRoom { get; set; }
		public bool IsPutdownAllowed { get; }
		public bool IsWearable { get; }

		public static IReadOnlyDictionary<ItemID, Item> CreateOrUpdateMap(IReadOnlyDictionary<ItemID, Item>? sourceMap, ICollection<Initializer> initializers, IReadOnlyDictionary<AnimateID, Animate> animates)
		{
			Dictionary<ItemID, Item> items = new Dictionary<ItemID, Item>(initializers.Count);

			var compoundInitializers = new List<Initializer>();
			var typedInitializers = new Dictionary<Initializer, Func<Initializer, IReadOnlyDictionary<AnimateID, Animate>, IReadOnlyDictionary<ItemID, Item>, Item>>();

			foreach (var initializer in initializers)
			{
				var createMethod = GetCreateMethod(initializer.ID);

				if (createMethod != null)
					typedInitializers[initializer] = createMethod;

				else if (initializer.UsableOn != null && initializer.UsableOn.Length > 0)
				{
					if (initializer.Components != null && initializer.Components.Length == 2)
						compoundInitializers.Add(initializer);
					else
						items[initializer.ID] = RegisterTexts(UsableItem.Create(sourceMap?[initializer.ID], initializer, animates), initializer);
				}
				else
					items[initializer.ID] = RegisterTexts(Create(sourceMap?[initializer.ID], initializer), initializer);
			}

			foreach (var initializer in compoundInitializers)
				items[initializer.ID] = RegisterTexts(CompoundItem.Create(initializer, animates, items), initializer);

			foreach (var initializerMethod in typedInitializers)
			{
				var initializer = initializerMethod.Key;

				items[initializer.ID] = RegisterTexts(initializerMethod.Value.Invoke(initializer, animates, items), initializer);
			}

			return items;
		}

		private static Item RegisterTexts(Item item, Initializer initializer)
		{
			Facilities.ItemTextsMap.LoadInitializer(initializer.ID, new TextsInitializer(TextType.Use, initializer));
			Facilities.ItemTextsMap.LoadInitializer(initializer.ID, new TextsInitializer(TextType.Combine, initializer));

			return item;
		}

		static Item Create(Item? sourceItem, Initializer initializer)
			=> new Item(
				initializer.ID,
				initializer.Name,
				initializer.Description,
				sourceItem?.CurrentRoom ?? initializer.StartRoom,
				initializer.Wearable,
				!initializer.BlockPutdown);

		protected Item(ItemID id, string name, string description, RoomID startRoom, bool isWearable, bool isPutdownAllowed)
			=> (ID, Name, Description, CurrentRoom, IsWearable, IsPutdownAllowed)
			= (id, name, description, startRoom, isWearable, isPutdownAllowed);

		public virtual Result Use() => Result.Failure(UsageTexts);

		protected StringValues UsageTexts
		{
			get => Facilities.ItemTextsMap[ID, TextType.Use];
		}
		private static Func<Initializer, IReadOnlyDictionary<AnimateID, Animate>, IReadOnlyDictionary<ItemID, Item>, Item>? GetCreateMethod(ItemID id) => id switch
		{
			ItemID.TNT => Tnt.Create,
			ItemID.Flashlight => Flashlight.Create,
			ItemID.Bandage => Bandage.Create,
			ItemID.Sword => Sword.Create,
			_ => null
		};

		public virtual Snapshot TakeSnapshot(Snapshot? snapshot = null)
		{
			if (snapshot == null)
				snapshot = new Snapshot();

			snapshot.ID = ID;
			snapshot.Room = CurrentRoom;

			return snapshot;
		}

		public virtual bool RestoreSnapshot(Snapshot snapshot)
		{
			if (snapshot.ID != ID)
				return false;

			CurrentRoom = snapshot.Room;
			return true;
		}


		public class Initializer
		{
			public ItemID ID { get; set; }
			public string Name { get; set; } = "";
			public string Description { get; set; } = "";
			public RoomID StartRoom { get; set; }

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			public	string[]? UseTexts { get; set; }

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
			public bool BlockPutdown { get; set; }

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
			public bool Wearable { get; set; }

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			public ItemID[]? Components { get; set; }

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			public string[]? CombineTexts { get; set; }

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			public AnimateID[]? UsableOn { get; set; } = null;

			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
			public bool KeepAfterUse { get; set; }
		}

		public class Snapshot
		{
			public ItemID ID { get; set; }
			public RoomID Room { get; set; }
		}

		public class TextsInitializer : KeyedTextsMap<ItemID, TextType>.IInitializer
		{
			private readonly Initializer _initializer;
			private readonly TextType _textType;

			public TextsInitializer(TextType textType, Initializer initializer)
				=> (_textType, _initializer) = (textType, initializer);

			public TextType ID 
				=> _textType;

			public string[]? Texts
				=> _textType switch
				{
					TextType.Combine => _initializer.CombineTexts,
					TextType.Use => _initializer.UseTexts,
					_ => null
				};
		}

		public enum TextType
		{
			Use,
			Combine
		}
	}

	interface IUsable<TUsableOn>
	{
		ICollection<TUsableOn> UsableOn { get; }
		Result UseOn(TUsableOn subject);
	}

	class UsableItem : Item, IUsable<Animate>
	{
		public ICollection<Animate> UsableOn { get; }
		bool KeepsAfterUse { get; }

		public static UsableItem Create(Item? sourceItem, Initializer initializer, IReadOnlyDictionary<AnimateID, Animate> animates)
			=> new UsableItem
			(
			initializer.ID,
			initializer.Name,
			initializer.Description,
			sourceItem?.CurrentRoom ?? initializer.StartRoom,
			initializer.UsableOn!.Select(animateID => animates[animateID]).ToArray(),
			initializer.Wearable,
			!initializer.BlockPutdown,
			initializer.KeepAfterUse
			);

		protected UsableItem
			(
			ItemID id,
			string name,
			string description,
			RoomID startRoom,
			ICollection<Animate> usableOn,
			bool isWearable,
			bool isPutdownAllowed,
			bool keepAfterUse
			)
			: base(id, name, description, startRoom, isWearable, isPutdownAllowed)
			=> (UsableOn, KeepsAfterUse) = (usableOn, keepAfterUse);

		public virtual Result UseOn(Animate animate)
		{
			if (!UsableOn.Contains(animate))
				return Use();

			var result = animate.ApplyItem(ID);

			if (result.IsSuccess && !KeepsAfterUse)
				StatusManager?.RemoveFromPossession(ID);

			return result;
		}
	}

	interface ICompound<T>
	{
		T Self { get; }
		ICollection<T> Components { get; }
		Result Combine(T first, T second);
	}

	class CompoundItem : UsableItem, ICompound<Item>
	{
		public ICollection<Item> Components { get; }
		protected StringValues CombineTexts => Facilities.ItemTextsMap[ID, TextType.Combine];

		public Item Self => this;

		public static CompoundItem Create(Initializer initializer, IReadOnlyDictionary<AnimateID, Animate> animates, IReadOnlyDictionary<ItemID, Item> items)
			=> new CompoundItem
			(
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

		protected CompoundItem
			(
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
			return !Components.Contains(first) || !Components.Contains(second) || first == second 
				? Result.Failure()
				: Result.Success(CombineTexts);
		}

		private enum TextID
		{
			CantCombineWithItself = 0,
		}
	}
}
