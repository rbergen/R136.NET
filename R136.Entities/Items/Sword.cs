using R136.Entities.General;
using R136.Entities.Global;
using System.Collections.Generic;
using System.Linq;

namespace R136.Entities.Items
{
	public class Sword : UsableItem, IContinuable
	{
#pragma warning disable IDE0060 // Remove unused parameter
		public static Sword FromInitializer(Initializer initializer, IDictionary<AnimateID, Animate> animates, IDictionary<ItemID, Item> items)
			=> new Sword(initializer.ID, initializer.Name, initializer.Description, initializer.StartRoom,
				initializer.UsableOn!.Select(animateID => animates[animateID]).ToArray(), initializer.Wearable, !initializer.BlockPutdown, initializer.KeepAfterUse);
#pragma warning restore IDE0060 // Remove unused parameter

		public Sword(
			ItemID id,
			string name,
			string description,
			RoomID startRoom,
			ICollection<Animate> usableOn,
			bool isWearable,
			bool isPutdownAllowed,
			bool keepAfterUse
		) : base(id, name, description, startRoom, usableOn, isWearable, isPutdownAllowed, keepAfterUse) { }

		public override Result UseOn(Animate animate)
		{
			if (animate is not StrikableAnimate strikable || !UsableOn.Contains(animate))
				return Use();

			List<string> texts = new List<string>();

			if (Facilities.Randomizer.NextDouble() > .7)
				texts.AddRangeIfNotNull(Facilities.TextsMap[this, (int)TextID.Miss]);

			else
			{
				texts.AddRangeIfNotNull(Facilities.TextsMap[this, (int)TextID.Hit]);

				if (strikable.Used(this))
					return Result.Success(texts);
			}

			if (strikable.StrikesLeft == 1)
			{
				texts.Add(string.Empty);
				texts.AddRangeIfNotNull(Facilities.TextsMap[this, (int)TextID.SeriouslyInjured]);
			}

			if (Facilities.Randomizer.NextDouble() > .3)
				return Result.Success(texts);

			texts.Add(string.Empty);
			texts.AddRangeIfNotNull(Facilities.TextsMap[this, (int)TextID.CanStrikeAgain]);
			return Result.ContinuationRequested(new ContinuationStatus(this, strikable), Facilities.Configuration.YesNoInputSpecs, texts);
		}

		public Result Continue(object statusData, string input)
		{
			if (statusData is Animate animate && input.ToLower() == Facilities.Configuration.YesInput)
				return UseOn(animate);

			return Result.Success();
		}

		private enum TextID
		{
			Miss,
			Hit,
			SeriouslyInjured,
			CanStrikeAgain
		}
	}
}
