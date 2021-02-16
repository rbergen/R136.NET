using R136.Entities.General;
using System.Collections.Generic;

namespace R136.Entities.Items
{
	public class Tnt : Item
	{
#pragma warning disable IDE0060 // Remove unused parameter
		public static Tnt FromInitializer(Initializer initializer, IDictionary<AnimateID, Animate> animates, IDictionary<ItemID, Item> items)
			=> new Tnt(initializer.ID, initializer.Name, initializer.Description, initializer.StartRoom, initializer.UseTexts, initializer.Wearable, !initializer.BlockPutdown);
#pragma warning restore IDE0060 // Remove unused parameter

		public Tnt(
			ItemID id,
			string name,
			string description,
			RoomID startRoom,
			ICollection<string>? useTexts,
			bool isWearable,
			bool isPutdownAllowed
		) : base(id, name, description, startRoom, useTexts, isWearable, isPutdownAllowed) { }

		public override Result Use()
		{
			StatusManager?.DecreaseHealth();

			return base.Use();
		}
	}
}
