using R136.Entities.General;
using R136.Entities.Global;
using System.Collections.Generic;

namespace R136.Entities.Items
{
	class Bandage : Item
	{
#pragma warning disable IDE0060 // Remove unused parameter
		public static Bandage FromInitializer
			(
			Initializer initializer, 
			IDictionary<AnimateID, Animate> animates, 
			IDictionary<ItemID, Item> items
			)
#pragma warning restore IDE0060 // Remove unused parameter
			=> new Bandage
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
			return base.Use();
		}

		private enum TextID
		{
			FullHealth
		}
	}
}
