using Microsoft.Extensions.Primitives;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Entities.Items;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace R136.Entities.CommandProcessors
{
	class GeneralCommandProcessor : CommandProcessor, IContinuable
	{
		private const string ContinuationKey = "QkztbraYG4TCSdtqayuU";

		public ICommandCallbacks? CommandCallbacks { get; set; }

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

		private static StringValues GetTexts<TIndex>(CommandID commandId, TIndex textId) where TIndex : Enum
			=> Facilities.CommandTextsMap.Get(commandId, textId);

		private static StringValues GetTexts<TIndex>(CommandID commandID, TIndex textId, string tag, string content) where TIndex : Enum
			=> GetTexts(commandID, textId).ReplaceInAll($"{{{tag}}}", content);

		private static StringValues GetTexts<TIndex>(TIndex id) where TIndex : Enum
			=> GetTexts(CommandID.End, id);

		private static void AddStatusTexts(List<string?> list, StatusTextID id)
			=> list.AddRangeIfNotNull(GetTexts(CommandID.Status, id));
		private static void AddStatusTexts(List<string?> list, StatusTextID id, string tag, string content)
			=> list.AddRangeIfNotNull(GetTexts(CommandID.Status, id, tag, content));

		private Result? ValidateEmptyParameters(string command, string? parameters)
			=> parameters == null ? null : Result.Error(Facilities.TextsMap.Get(this, TextID.Default).ReplaceInAll("{command}", command));

		private Result ExecuteHelp()
		{
			var helpTexts = GetTexts(CommandID.Help, TextID.Default);
			if (CommandCallbacks != null)
				helpTexts = StringValues.Concat(helpTexts, CommandCallbacks.AdditionalHelpTexts);

            return Result.Success(helpTexts);
		}

		private Result ExecuteInfo(string command, string? parameters)
		{
			var validateResult = ValidateEmptyParameters(command, parameters);

			if (validateResult != null)
				return validateResult;

			var assembly = typeof(EntityBase).Assembly;
			var version = assembly.GetName().Version;
			string versionText = version != null ? $"{version.Major}.{version.Minor}" : "?.?";
			string? copyrightText = ((AssemblyCopyrightAttribute?)assembly.GetCustomAttributes(false).FirstOrDefault(attribute => attribute is AssemblyCopyrightAttribute))?.Copyright;

			return Result.Success(GetTexts(CommandID.Info, TextID.Default, "version", versionText).ReplaceInAll("{copyright}", copyrightText ?? string.Empty));
		}

		private Result ExecuteWait(string command, string? parameters)
		{
			var validateResult = ValidateEmptyParameters(command, parameters);

			if (validateResult != null)
				return validateResult;

			var waitTexts = GetTexts(CommandID.Wait, TextID.Default).ToArray();

			return waitTexts == null ? Result.Success() : Result.Success(waitTexts[Facilities.Randomizer.Next(waitTexts.Length)] ?? string.Empty);
		}

		private Result ExecuteStatus(string command, string? parameters, Player player)
		{
			var validateResult = ValidateEmptyParameters(command, parameters);

			if (validateResult != null)
				return validateResult;

			List<string?> texts = new();

			AddStatusTexts(texts, StatusTextID.Header);
			AddStatusTexts(texts, StatusTextID.LifePoints, "lifepoints", player.LifePoints.ToString());

			if (player.Inventory.Count == 0)
				AddStatusTexts(texts, StatusTextID.NoInventory);

			else
			{
				if (player.FindInInventory(ItemID.Flashlight) is Flashlight flashlight)
				{
					AddStatusTexts(texts, flashlight.IsOn ? StatusTextID.FlashlightOn : StatusTextID.FlashlightOff);
					if (flashlight.HasBatteries)
						AddStatusTexts(texts, StatusTextID.FlashlightHasBatteries);
				}

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
				return Result.InputRequested
					(
						new() { Key = ContinuationKey },
						Facilities.Configuration.YesNoInputSpecs,
						GetTexts(EndTextID.InvalidYesNoAnswer
					)
				);

			return input == Facilities.Configuration.YesInput
				? Result.EndRequested()
				: Result.Success(GetTexts(EndTextID.EndCancelled));
		}

		private enum StatusTextID : byte
		{
			Header,
			LifePoints,
			FlashlightOn,
			FlashlightOff,
			NoInventory,
			InventoryHeader,
			InventoryItem,
			FlashlightHasBatteries
		}

		private enum EndTextID : byte
		{
			ConfirmEnd,
			EndCancelled,
			InvalidYesNoAnswer
		}

		private enum TextID : byte
		{
			Default
		}
	}
}
