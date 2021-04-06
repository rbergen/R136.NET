using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System.Collections.Generic;

namespace R136.Entities.Items
{
	class Bandage : Item
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are part of delegate interface")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Legibility")]
		public static Bandage Create
			(
				Initializer initializer,
				IReadOnlyDictionary<AnimateID, Animate> animates,
				IReadOnlyDictionary<ItemID, Item> items
			)
			=> new			
			(
				initializer.ID,
				initializer.Name,
				initializer.Description,
				initializer.StartRoom,
				initializer.Wearable,
				!initializer.BlockPutdown
			);

		private Bandage
			(
				ItemID id,
				string name,
				string description,
				RoomID startRoom,
				bool isWearable,
				bool isPutdownAllowed
			) : base(id, name, description, startRoom, isWearable, isPutdownAllowed) { }

		public override Result Use()
		{
			if (Player != null && Player.LifePoints == Facilities.Configuration.LifePoints)
				return Result.Success(Facilities.TextsMap.Get(this, TextID.FullHealth));

			Player?.RestoreHealth();

			return Result.Success(UsageTexts, true);
		}

		private enum TextID : byte
		{
			FullHealth
		}
	}
}
