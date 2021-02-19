using R136.Entities.General;
using R136.Entities.Global;
using System;
using System.Collections.Generic;
using System.Linq;

namespace R136.Entities.Items
{
	class Sword : UsableItem, IContinuable
	{
#pragma warning disable IDE0060 // Remove unused parameter
		public static Sword FromInitializer(Initializer initializer, IDictionary<AnimateID, Animate> animates, IDictionary<ItemID, Item> items)
			=> new Sword
			(
			initializer.ID, 
			initializer.Name, 
			initializer.Description, 
			initializer.StartRoom,
			initializer.UsableOn!.Select(animateID => animates[animateID]).ToArray(), 
			initializer.Wearable, 
			!initializer.BlockPutdown, 
			initializer.KeepAfterUse
			);
#pragma warning restore IDE0060 // Remove unused parameter

		private Sword
			(
			ItemID id,
			string name,
			string description,
			RoomID startRoom,
			ICollection<Animate> usableOn,
			bool isWearable,
			bool isPutdownAllowed,
			bool keepAfterUse
			) 
			: base(id, name, description, startRoom, usableOn, isWearable, isPutdownAllowed, keepAfterUse) { }

		public override Result UseOn(Animate animate)
		{
			if (animate is not StrikableAnimate strikable || !UsableOn.Contains(animate))
				return Use();

			var texts = new List<string>();

			if (Facilities.Randomizer.NextDouble() > .7)
				AddTexts(texts, TextID.Miss);

			else
			{
				AddTexts(texts, TextID.Hit);

				if (strikable.Used(this).IsSuccess)
					return Result.Success(texts);
			}

			if (strikable.StrikesLeft == 1)
			{
				texts.Add(string.Empty);
				AddTexts(texts, TextID.SeriouslyInjured);
			}

			if (Facilities.Randomizer.NextDouble() > .3)
				return Result.Success(texts);

			texts.Add(string.Empty);
			AddTexts(texts, TextID.CanStrikeAgain);
			return Result.ContinuationRequested(new ContinuationStatus(this, (this, strikable)), Facilities.Configuration.YesNoInputSpecs, texts);
		}

		private void AddTexts(List<string> texts, TextID id)
			=> texts.AddRangeIfNotNull(Facilities.TextsMap[this, (int)id]);

		public Result Continue(object statusData, string input)
		{
			if (statusData is ValueTuple<Sword, StrikableAnimate> statusTuple && statusTuple.Item1 == this)
				return input.ToLower() == Facilities.Configuration.YesInput ? UseOn(statusTuple.Item2) : Result.Success();

			return Result.Failure();
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
