﻿using R136.Entities.General;
using R136.Entities.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.CommandProcessors
{
	class ItemCommandProcessor : ActingCommandProcessor
	{
		private const int Default = 0;

		public ItemCommandProcessor(IReadOnlyDictionary<ItemID, Item> items, IReadOnlyDictionary<AnimateID, Animate> animates) : base(items, animates) { }

		public override Result Execute(CommandID id, string command, string? parameters, Player player)
			=> id switch
			{
				CommandID.Use => ExecuteUse(command, parameters, player),
				CommandID.Combine => ExecuteCombine(command, parameters, player),
				CommandID.Pickup => ExecutePickup(command, parameters, player),
				CommandID.PutDown => ExecutePutDown(command, parameters, player),
				CommandID.Inspect => ExecuteInspect(command, parameters, player),
				_ => Result.Error()
			};

		private Result ExecutePickup(string command, string? parameters, Player player)
		{
			(Item? item, Result findResult) = FindPresentItem(command, parameters, player);

			if (item == null)
				return findResult;

			var result = player.AddToInventory(item);

			if (result.IsSuccess)
				item.CurrentRoom = RoomID.None;

			return result;
		}

		private Result ExecuteInspect(string command, string? parameters, Player player)
		{
			(Item? item, Result result) = FindOwnedItem(command, parameters, player);

			return item == null
				? result
				: (StatusManager?.IsDark ?? false)
        ? Result.Failure(GetTexts(CommandID.Inspect, Default))
        : Result.Success(new string[] { item.Description });
		}

		private Result ExecutePutDown(string command, string? parameters, Player player)
		{
			(Item? item, Result result) = FindOwnedItem(command, parameters, player);

			if (item == null)
				return result;

			if (!item.IsPutdownAllowed)
				return Result.Failure(GetTextsWithItem(CommandID.PutDown, (int)PutDownTextID.CantPutDown, item));

			if (player.RemoveFromInventory(item))
			{
				item.CurrentRoom = player.CurrentRoom.ID;
				return Result.Success(GetTextsWithItem(CommandID.PutDown, (int)PutDownTextID.PutDown, item));
			}

			return Result.Error();
		}

		private Result ExecuteCombine(string command, string? parameters, Player player)
		{
			if (parameters == null)
				return Result.Error(GetTexts(CombineTextID.NoParametersGiven, "command", command));

			(var separatorIndex, var separator) = parameters.IndexOfAny(GetTexts(CommandID.Combine, (int)CombineTextID.ItemSeparators));
			if (separatorIndex < 1 || separatorIndex + separator!.Length == parameters.Length)
				return Result.Error(GetTexts(CombineTextID.NoParametersGiven, "command", command));

			var itemNames = new string[]
			{
				parameters[..separatorIndex].Trim(),
				parameters[(separatorIndex + separator.Length)..].Trim()
			};

			if (itemNames.Any(s => s == string.Empty))
				return Result.Error(GetTexts(CombineTextID.NoParametersGiven, "command", command));

			var items = new Item?[itemNames.Length];
			Result result;

			for (int i = 0; i < itemNames.Length; i++)
			{
				(items[i], result) = FindOwnedItem(command, itemNames[i], player);
				if (items[i] == null)
					return result;
			}

			if (items.Distinct().Count() != items.Length)
				return Result.Failure(GetTexts(CombineTextID.CantCombineWithSelf));

			var combineItem = Items.Values
				.Where(item => item is ICompound<Item>)
				.Cast<ICompound<Item>>()
				.FirstOrDefault(combineItem => combineItem.Components.Intersect(items).Count() == combineItem.Components.Count);

			if (combineItem == null)
				return Result.Failure(GetTexts(CombineTextID.DoesntCombine));

			foreach (var item in items)
				player.RemoveFromInventory(item!);

			result = combineItem.Combine(items[0]!, items[1]!);

			player.AddToInventory(combineItem.Self);

			return result;
		}

		private Result ExecuteUse(string command, string? parameters, Player player)
		{
			(Item? item, Result result) = FindOwnedItem(command, parameters, player);

			if (item == null)
				return result;

			var presentAnimate = Animates.Values.FirstOrDefault(animate => animate.CurrentRoom == player.CurrentRoom.ID);

			if (presentAnimate == null || item is not UsableItem usableItem || !usableItem.UsableOn.Contains(presentAnimate))
				return item.Use();

			return usableItem.UseOn(presentAnimate);
		}

		private ICollection<string>? GetTexts(TextID id)
			=> Facilities.TextsMap[this, (int)id];

		private ICollection<string>? GetTexts(TextID id, string tag, string parameter)
			=> GetTexts(id).ReplaceInAll($"{{{tag}}}", parameter);

		private static ICollection<string>? GetTexts(CommandID commandId, int textId)
			=> Facilities.CommandTextsMap[commandId, textId];

		private static ICollection<string>? GetTextsWithItem(CommandID commandID, int textId, Item item)
			=> GetTexts(commandID, textId).ReplaceInAll("{item}", item.Name);

		private static ICollection<string>? GetTexts(CombineTextID id, string tag, string content)
			=> GetTexts(id).ReplaceInAll($"{{{tag}}}", content);

		private static ICollection<string>? GetTexts(CombineTextID id)
			=> GetTexts(CommandID.Combine, (int)id);

		private (Item?, Result) FindOwnedItem(string command, string? itemName, Player player)
		{
			if (itemName == null || itemName == string.Empty)
				return (null, Result.Error(GetTexts(TextID.NoParameterGiven, "command", command)));

			(Item? item, FindResult result) = player.FindInInventory(itemName);

			return result switch
			{
				FindResult.NotFound => (null, Result.Error(GetTexts(TextID.DontOwnParameter, "param", itemName))),
				FindResult.Ambiguous => (null, Result.Error(GetTexts(TextID.ParameterAmbiguous, "param", itemName))),
				_ => (item, Result.Success())
			};
		}

		private (Item?, Result) FindPresentItem(string command, string? parameters, Player player)
		{
			if (parameters == null || parameters == string.Empty)
				return (null, Result.Error(GetTexts(TextID.NoParameterGiven, "command", command)));

			(Item? item, FindResult result) = Items.Values.Where(item => item.CurrentRoom == player.CurrentRoom.ID).ToList().FindItemByName(parameters);

			return result switch
			{
				FindResult.NotFound => (null, Result.Error(GetTexts(TextID.ParameterNotPresent, "param", parameters))),
				FindResult.Ambiguous => (null, Result.Error(GetTexts(TextID.ParameterAmbiguous, "param", parameters))),
				_ => (item, Result.Success())
			};
		}

		private enum TextID
		{
			NoParameterGiven,
			DontOwnParameter,
			ParameterAmbiguous,
			ParameterNotPresent,
		}

		private enum CombineTextID
		{
			NoParametersGiven,
			ItemSeparators,
			CantCombineWithSelf,
			DoesntCombine
		}

		private enum PutDownTextID
		{
			CantPutDown,
			PutDown
		}
	}
}
