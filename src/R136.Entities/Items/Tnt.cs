using R136.Interfaces;
using System.Collections.Generic;

namespace R136.Entities.Items
{
	class Tnt : Item
	{
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are part of delegate interface")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Legibility")]
		public static Tnt Create
			(
				Initializer initializer,
				IReadOnlyDictionary<AnimateID, Animate> animates,
				IReadOnlyDictionary<ItemID, Item> items
			)
			=> new(initializer.ID, initializer.Name, initializer.Description, initializer.StartRoom, initializer.Wearable, !initializer.BlockPutdown);

		private Tnt
			(
				ItemID id,
				string name,
				string description,
				RoomID startRoom,
				bool isWearable,
				bool isPutdownAllowed
			)
			: base(id, name, description, startRoom, isWearable, isPutdownAllowed) { }

		public override Result Use()
		{
			Player?.DecreaseHealth();

			return base.Use();
		}
	}
}
