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
		public class Initializer
		{
			public string Name { get; set; }
			public string Description { get; set; }
			public RoomID StartRoom { get; set; }
			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			public ItemID? CombinesWith { get; set; } = null;
			[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
			public AnimateID[] WorksOn { get; set; } = null;
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
