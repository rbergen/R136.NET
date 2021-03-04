using R136.Entities.Global;
using R136.Interfaces;
using System.Collections.Generic;

namespace R136.Entities.Items
{
	class Bandage : Item
	{
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
			if (StatusManager != null && StatusManager.LifePoints == Facilities.Configuration.LifePoints)
				return Result.Success(Facilities.TextsMap[this, (int)TextID.FullHealth]);

			StatusManager?.RestoreHealth();

			return Result.Success(UsageTexts);
		}

		private enum TextID
		{
			FullHealth
		}
	}
}
