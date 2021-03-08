using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace R136.Entities.Items
{
	class Sword : UsableItem, IContinuable
	{
		private const string ContinuationKey = "myp3ybBgaJznuoTCcRpj";

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameters are part of delegate interface")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0079:Remove unnecessary suppression", Justification = "Legibility")]
		public static Sword Create(Initializer initializer, IReadOnlyDictionary<AnimateID, Animate> animates, IReadOnlyDictionary<ItemID, Item> items)
			=> new
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

				if (strikable.ApplyItem(this).IsSuccess)
					return Result.Success(texts.ToArray());
			}

			if (strikable.StrikesLeft == 1)
			{
				texts.Add(string.Empty);
				AddTexts(texts, TextID.SeriouslyInjured);
			}

			if (Facilities.Randomizer.NextDouble() > .3)
				return Result.Failure(texts.ToArray(), true);

			texts.Add(string.Empty);
			AddTexts(texts, TextID.CanStrikeAgain);
			return Result.InputRequested
			(
				new() { Key = ContinuationKey, Number = (int)strikable.ID },
				Facilities.Configuration.YesNoInputSpecs,
				texts.ToArray()
			);
		}

		private void AddTexts(List<string> texts, TextID id)
			=> texts.AddRangeIfNotNull(Facilities.TextsMap[this, (int)id]);

		public Result Continue(ContinuationStatus status, string input)
		{
			if (status.Key != ContinuationKey || status.Number == null)
				return Result.Error();

			var animate = UsableOn.FirstOrDefault(animate => animate.ID == (AnimateID)status.Number);
			if (animate == null)
				return Result.Error();

			var yesNoInputSpecs = Facilities.Configuration.YesNoInputSpecs;
			if (yesNoInputSpecs.Permitted != null && yesNoInputSpecs.MaxLength == 1 && !yesNoInputSpecs.Permitted.Contains(input))
				return Result.InputRequested
				(
					new() { Key = ContinuationKey, Number = (int)animate.ID },
					Facilities.Configuration.YesNoInputSpecs,
					Facilities.TextsMap[this, (int)TextID.InvalidYesNoAnswer]
				);

			return input.ToLower() == Facilities.Configuration.YesInput ? UseOn(animate) : Result.Failure();
		}

		private enum TextID
		{
			Miss,
			Hit,
			SeriouslyInjured,
			CanStrikeAgain,
			InvalidYesNoAnswer
		}
	}
}
