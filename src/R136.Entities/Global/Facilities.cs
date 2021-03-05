using R136.Entities.General;
using R136.Interfaces;
using System;

namespace R136.Entities.Global
{
	public static class Facilities
	{
		public static IServiceProvider? Services { get; set; }
		public static Random Randomizer { get; }
		public static TypedTextsMap<int> TextsMap { get; }
		public static TypedTextsMap<AnimateStatus> AnimateStatusTextsMap { get; }
		public static KeyedTextsMap<ItemID, Item.TextType> ItemTextsMap { get; }
		public static KeyedTextsMap<CommandID, int> CommandTextsMap { get; }
		public static Configuration Configuration { get; set; }
		public static Logger Logger { get; }

		static Facilities()
		{
			Randomizer = new();
			TextsMap = new();
			AnimateStatusTextsMap = new();
			ItemTextsMap = new();
			CommandTextsMap = new();
			Configuration = new();
			Logger = new();
		}

	}
}
