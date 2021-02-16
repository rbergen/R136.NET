using R136.Entities.General;
using R136.Entities.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.Items
{
	public class Bandage : Item
	{
#pragma warning disable IDE0060 // Remove unused parameter
		public static Bandage FromInitializer(Initializer initializer, IDictionary<AnimateID, Animate> animates, IDictionary<ItemID, Item> items)
			=> new Bandage(initializer.ID, initializer.Name, initializer.Description, initializer.StartRoom, initializer.UseTexts, initializer.Wearable, !initializer.BlockPutdown);
#pragma warning restore IDE0060 // Remove unused parameter

		public Bandage(
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
			if (StatusManager != null && StatusManager.LifePoints == Facilities.Configuration.LifePoints)
				return new Result(ResultCode.Success, Facilities.TextsMap[this, (int)TextID.FullHealth]);

			StatusManager?.RestoreHealth();
			return base.Use();
		}

		private enum TextID
		{
			FullHealth
		}
	}
}
