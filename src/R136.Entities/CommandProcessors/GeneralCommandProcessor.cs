using Microsoft.Extensions.Primitives;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Entities.Items;
using R136.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace R136.Entities.CommandProcessors
{
	class GeneralCommandProcessor : CommandProcessor, IContinuable
	{
		private const int Default = 0;
		private const string ContinuationKey = "QkztbraYG4TCSdtqayuU";

		public GeneralCommandProcessor() : base(CommandProcessorID.General) { }

		public override Result Execute(CommandID id, string command, string? parameters, Player player)
			=> id switch
			{
				CommandID.Help => ExecuteHelp(),
				CommandID.Wait => ExecuteWait(command, parameters),
				CommandID.Status => ExecuteStatus(command, parameters, player),
				CommandID.End => ExecuteEnd(command, parameters),
				CommandID.Info => ExecuteInfo(command, parameters),
				_ => Result.Error()
			};

		private static StringValues GetTexts(CommandID commandId, int textId)
			=> Facilities.CommandTextsMap[commandId, textId];

		private static StringValues GetTexts(CommandID commandID, int textId, string tag, string content)
			=> GetTexts(commandID, textId).ReplaceInAll($"{{{tag}}}", content);

		private static StringValues GetTexts(EndTextID id)
			=> GetTexts(CommandID.End, (int)id);

		private static void AddStatusTexts(List<string> list, StatusTextID id)
			=> list.AddRangeIfNotNull(GetTexts(CommandID.Status, (int)id));
		private static void AddStatusTexts(List<string> list, StatusTextID id, string tag, string content)
			=> list.AddRangeIfNotNull(GetTexts(CommandID.Status, (int)id, tag, content));

		private Result? ValidateEmptyParameters(string command, string? parameters)
			=> parameters == null ? null : Result.Error(Facilities.TextsMap[this, (int)TextID.CommandSyntax].ReplaceInAll("{command}", command));

		private static Result ExecuteHelp()
			=> Result.Success(GetTexts(CommandID.Help, Default));

		private Result ExecuteInfo(string command, string? parameters)
		{
			var validateResult = ValidateEmptyParameters(command, parameters);

			if (validateResult != null)
				return validateResult;

			var assembly = typeof(EntityBase).Assembly;
			var version = assembly.GetName().Version;
			var versionText = version != null ? $"{version.Major}.{version.Minor}" : "?.?";
			var copyrightText = ((AssemblyCopyrightAttribute?)assembly.GetCustomAttributes(false).FirstOrDefault(attribute => attribute is AssemblyCopyrightAttribute))?.Copyright;

			return Result.Success(GetTexts(CommandID.Info, Default, "version", versionText).ReplaceInAll("{copyright}", copyrightText ?? string.Empty));
		}

		private Result ExecuteWait(string command, string? parameters)
		{
			var validateResult = ValidateEmptyParameters(command, parameters);

			if (validateResult != null)
				return validateResult;

			var waitTexts = GetTexts(CommandID.Wait, Default).ToArray();

			return waitTexts == null ? Result.Success() : Result.Success(waitTexts[Facilities.Randomizer.Next(waitTexts.Length)]);
		}

		private Result ExecuteStatus(string command, string? parameters, Player player)
		{
			var validateResult = ValidateEmptyParameters(command, parameters);

			if (validateResult != null)
				return validateResult;

			var texts = new List<string>();

			AddStatusTexts(texts, StatusTextID.Header);
			AddStatusTexts(texts, StatusTextID.LifePoints, "lifepoints", player.LifePoints.ToString());

			if (player.Inventory.Count == 0)
				AddStatusTexts(texts, StatusTextID.NoInventory);

			else
			{
				if (player.FindInInventory(ItemID.Flashlight) is Flashlight flashlight)
					AddStatusTexts(texts, flashlight.IsOn ? StatusTextID.FlashlightOn : StatusTextID.FlashlightOff);

				AddStatusTexts(texts, StatusTextID.InventoryHeader);
				foreach (var item in player.Inventory)
					AddStatusTexts(texts, StatusTextID.InventoryItem, "item", item.Name);
			}

			return Result.Success(texts.ToArray());
		}

		private Result ExecuteEnd(string command, string? parameters)
		{
			var validateResult = ValidateEmptyParameters(command, parameters);

			if (validateResult != null)
				return validateResult;

			return Result.InputRequested(new ContinuationStatus() { Key = ContinuationKey }, Facilities.Configuration.YesNoInputSpecs, GetTexts(EndTextID.ConfirmEnd));
		}

		public Result Continue(ContinuationStatus status, string input)
		{
			if (status.Key != ContinuationKey)
				return Result.Error();

			var yesNoInputSpecs = Facilities.Configuration.YesNoInputSpecs;
			if (yesNoInputSpecs.Permitted != null && yesNoInputSpecs.MaxLength == 1 && !yesNoInputSpecs.Permitted.Contains(input))
				return Result.InputRequested(
					new ContinuationStatus() { Key = ContinuationKey },
					Facilities.Configuration.YesNoInputSpecs,
					GetTexts(EndTextID.InvalidYesNoAnswer)
				);

			return input == Facilities.Configuration.YesInput
				? Result.EndRequested()
				: Result.Success(GetTexts(EndTextID.EndCancelled));
		}

		private enum StatusTextID
		{
			Header,
			LifePoints,
			FlashlightOn,
			FlashlightOff,
			NoInventory,
			InventoryHeader,
			InventoryItem
		}

		private enum EndTextID
		{
			ConfirmEnd,
			EndCancelled,
			InvalidYesNoAnswer
		}

		private enum TextID
		{
			CommandSyntax
		}
	}
}
