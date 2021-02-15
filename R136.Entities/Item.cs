using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace R136.Entities
{
	public class Item
	{
		public ItemID ID { get; private set; }
		public string Name { get; }
		public string? Description { get; }
		public RoomID CurrentRoom { get; set; }

		private IStatusManager? _statusManager = null;
		private IServiceProvider? _serviceProvider;

		public static IDictionary<ItemID, Item> FromInitializers(IServiceProvider serviceProvider, ICollection<Initializer> initializers, IDictionary<AnimateID, Animate> animates)
		{
			Dictionary<ItemID, Item> items = new Dictionary<ItemID, Item>(initializers.Count);
			Item item;

			foreach(var initializer in initializers)
			{
				if (initializer.ComponentItems != null && initializer.ComponentItems.Length == 2)
					continue;
				else if (initializer.ActsOn != null && initializer.ActsOn.Length > 0)
				{
					item = new ActingItem(serviceProvider, initializer.Name, initializer.Description, initializer.StartRoom,
						initializer.ActsOn.Select(animateID => animates[animateID]).ToArray()); 
				}
				else
					item = new Item(serviceProvider, initializer.Name, initializer.Description, initializer.StartRoom);

				item.ID = initializer.ID;
				items[initializer.ID] = item;
			}



			return items;
		}

		public Item(IServiceProvider serviceProvider, string name, string? description, RoomID startRoom)
			=> (_serviceProvider, Name, Description, CurrentRoom) = (serviceProvider, name, description, startRoom);

		public class Initializer
		{
			public ItemID ID { get; set; }
			public string Name { get; set; } = "";
			public string? Description { get; set; }
			public RoomID StartRoom { get; set; }
			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			public ItemID[]? ComponentItems { get; set; }
			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			public AnimateID[]? ActsOn { get; set; } = null;
		}
	}

	public class CompoundItem : Item {
		public ICollection<Item> ComponentItems;

		public CompoundItem(IServiceProvider serviceProvider, string name, string? description, RoomID startRoom, ICollection<Item> compoundItems)
			: base(serviceProvider, name, description, startRoom) => ComponentItems = compoundItems;

	}

	public class ActingItem : Item
	{
		public ICollection<Animate> ActsOn;

		public ActingItem(IServiceProvider serviceProvider, string name, string? description, RoomID startRoom, ICollection<Animate> actsOn)
			: base(serviceProvider, name, description, startRoom) => ActsOn = actsOn;

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

	public static class ItemTypeMap
	{
		private static readonly Dictionary<ItemID, Type> _map = new Dictionary<ItemID, Type>(Enum.GetValues(typeof(ItemID)).Length)
		{
//			[ItemID.Sword] = typeof(Sword),
//			[ItemID.RedTroll] = typeof(RedTroll),
			[AnimateID.Plant] = typeof(Plant),
			[AnimateID.Gnu] = typeof(Gnu),
			[AnimateID.Dragon] = typeof(Dragon),
			[AnimateID.Swelling] = typeof(Swelling),
			[AnimateID.Door] = typeof(Door),
			[AnimateID.Voices] = typeof(Voices),
			[AnimateID.Barbecue] = typeof(Barbecue),
			[AnimateID.Tree] = typeof(Tree),
			[AnimateID.GreenCrystal] = typeof(GreenCrystal),
			[AnimateID.Computer] = typeof(Computer),
			[AnimateID.DragonHead] = typeof(DragonHead),
			[AnimateID.Lava] = typeof(Lava),
			[AnimateID.Vacuum] = typeof(Vacuum),
			[AnimateID.PaperHatch] = typeof(PaperHatch),
			[AnimateID.SwampBase] = typeof(Swamp),
			[AnimateID.NorthSwamp] = typeof(Swamp),
			[AnimateID.MiddleSwamp] = typeof(Swamp),
			[AnimateID.SouthSwamp] = typeof(Swamp),
			[AnimateID.Mist] = typeof(Mist),
			[AnimateID.Teleporter] = typeof(Teleporter)
		};

		public static Type FromID(AnimateID id) => _map[id];
	}
}
