using Microsoft.Extensions.Primitives;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace R136.Entities.CommandProcessors
{
	class ItemCommandProcessor : CommandProcessor, IContinuable
	{
		private const string ContinuationKey = "M54ym08ugrEYkp4tpTu1";

		protected IReadOnlyDictionary<ItemID, Item> _items;
		protected IReadOnlyDictionary<AnimateID, Animate> _animates;


		public ItemCommandProcessor(IReadOnlyDictionary<ItemID, Item> items, IReadOnlyDictionary<AnimateID, Animate> animates) : base(CommandProcessorID.Item)
			=> (_items, _animates) = (items, animates);

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
				: (Player?.IsDark ?? false)
				? Result.Failure(GetTexts(CommandID.Inspect, InspectTextID.TooDarkToSee))
				: Result.Success(item.Description, true);
		}

		private Result ExecutePutDown(string command, string? parameters, Player player)
		{
			(Item? item, Result result) = FindOwnedItem(command, parameters, player);

			if (item == null)
				return result;

			if (!item.IsPutdownAllowed)
				return Result.Failure(GetTextsWithItem(CommandID.PutDown, PutDownTextID.CantPutDown, item), true);

			if (player.RemoveFromInventory(item))
			{
				item.CurrentRoom = player.CurrentRoom.ID;
				return Result.Success(GetTextsWithItem(CommandID.PutDown, PutDownTextID.PutDown, item));
			}

			return Result.Error();
		}

		private Result ExecuteCombine(string command, string? parameters, Player player)
		{
			if (parameters == null)
				return Result.Error(GetTexts(CombineTextID.InvalidParametersGiven, "command", command));

			(var separatorIndex, var separator) = parameters.IndexOfAny(GetTexts(CommandID.Combine, CombineTextID.ItemSeparators));
			if (separatorIndex < 1 || separatorIndex + separator!.Length == parameters.Length)
				return Result.Error(GetTexts(CombineTextID.InvalidParametersGiven, "command", command));

			var itemNames = new string[]
			{
				parameters[..separatorIndex].Trim(),
				parameters[(separatorIndex + separator.Length)..].Trim()
			};

			if (itemNames.Any(s => s == string.Empty))
				return Result.Error(GetTexts(CombineTextID.InvalidParametersGiven, "command", command));

			var items = new Item?[itemNames.Length];
			Result result;

			for (int i = 0; i < itemNames.Length; i++)
			{
				(items[i], result) = FindOwnedItem(command, itemNames[i], player);
				if (items[i] == null)
					return result;
			}

			if (items.Distinct().Count() != items.Length)
				return Result.Failure(GetTexts(CombineTextID.CantCombineWithItself), true);

			var combineItem = _items.Values
				.OfType<ICompound<Item>>()
				.FirstOrDefault(combineItem => combineItem.Components.Intersect(items).Count() == combineItem.Components.Count);

			if (combineItem == null)
				return Result.Failure(GetTexts(CombineTextID.DoesntCombine), true);

			foreach (var item in items)
				player.RemoveFromInventory(item!);

			result = combineItem.Combine(items[0]!, items[1]!);

			player.AddToInventory(combineItem.Self);

			result.PauseRequested = true;
			return result;
		}

		private Result ExecuteUse(string command, string? parameters, Player player)
		{
			(var item, var result) = FindOwnedItem(command, parameters, player);

			if (item == null)
				return result;

			var presentAnimate = _animates.Values.FirstOrDefault(animate => animate.CurrentRoom == player.CurrentRoom.ID);

			if (presentAnimate == null || item is not UsableItem usableItem || !usableItem.UsableOn.Contains(presentAnimate))
				return item.Use().WrapInputRequest(ContinuationKey, (int)item.ID);

			return usableItem.UseOn(presentAnimate).WrapInputRequest(ContinuationKey, (int)item.ID);
		}

		private StringValues GetTexts(TextID id)
			=> Facilities.TextsMap.Get(this, id);

		private StringValues GetTexts(TextID id, string tag, string parameter)
			=> GetTexts(id).ReplaceInAll($"{{{tag}}}", parameter);

		private static StringValues GetTexts<TIndex>(CommandID commandId, TIndex textId) where TIndex : Enum
			=> Facilities.CommandTextsMap.Get(commandId, textId);

		private static StringValues GetTextsWithItem<TIndex>(CommandID commandID, TIndex textId, Item item) where TIndex : Enum
			=> GetTexts(commandID, textId).ReplaceInAll("{item}", item.Name);

		private static StringValues GetTexts(CombineTextID id, string tag, string content)
			=> GetTexts(id).ReplaceInAll($"{{{tag}}}", content);

		private static StringValues GetTexts(CombineTextID id)
			=> GetTexts(CommandID.Combine, id);

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

			(Item? item, FindResult result) = _items.Values.Where(item => item.CurrentRoom == player.CurrentRoom.ID).ToList().FindItemByName(parameters);

			return result switch
			{
				FindResult.NotFound => (null, Result.Error(GetTexts(TextID.ParameterNotPresent, "param", parameters))),
				FindResult.Ambiguous => (null, Result.Error(GetTexts(TextID.ParameterAmbiguous, "param", parameters))),
				_ => (item, Result.Success())
			};
		}

		public Result Continue(ContinuationStatus status, string input)
		{
			return Result.ContinueWrappedContinuationStatus(ContinuationKey, status, input, DoContinuation);

			Result DoContinuation(ContinuationStatus status, string input)
			{
				if (status.Number != null && _items[(ItemID)status.Number] is IContinuable item)
					return item.Continue(status.InnerStatus!, input).WrapInputRequest(ContinuationKey, status.Number.Value);

				return Result.Error();
			}
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
			InvalidParametersGiven,
			ItemSeparators,
			CantCombineWithItself,
			DoesntCombine
		}

		private enum InspectTextID
		{
			TooDarkToSee
		}

		private enum PutDownTextID
		{
			CantPutDown,
			PutDown
		}
	}
}
