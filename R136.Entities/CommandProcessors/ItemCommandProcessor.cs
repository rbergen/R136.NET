using R136.Entities.General;
using R136.Entities.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities.CommandProcessors
{
	class ItemCommandProcessor : CommandProcessor
	{
		private const int Default = 0;

		private readonly IReadOnlyDictionary<ItemID, Item> _items;

		public ItemCommandProcessor(IReadOnlyDictionary<ItemID, Item> items)
			=> _items = items;

		public override Result Execute(CommandID id, string name, string? parameters, Player player, ICollection<Item> presentItems, Animate? presentAnimate)
			=> id switch
			{
				CommandID.Use => ExecuteUse(name, parameters, player, presentAnimate),
				CommandID.Combine => ExecuteCombine(name, parameters, player),
				CommandID.Pickup => ExecutePickup(name, parameters, player, presentItems),
				CommandID.PutDown => ExecutePutDown(name, parameters, player),
				CommandID.Inspect => ExecuteInspect(name, parameters, player),
				_ => Result.Error()
			};

		private Result ExecutePickup(string name, string? parameters, Player player, ICollection<Item> presentItems)
		{
			(Item? item, Result findResult) = FindPresentItem(name, parameters, presentItems);

			if (item == null)
				return findResult;

			var result = player.AddToInventory(item);

			if (result.IsSuccess)
				item.CurrentRoom = RoomID.None;

			return result;
		}

		private Result ExecuteInspect(string name, string? parameters, Player player)
		{
			(Item? item, Result result) = FindOwnedItem(name, parameters, player);

			return item == null
				? result
				: (StatusManager?.IsDark ?? false)
        ? Result.Failure(GetTexts(CommandID.Inspect, Default))
        : Result.Success(new string[] { item.Description });
		}

		private Result ExecutePutDown(string name, string? parameters, Player player)
		{
			(Item? item, Result result) = FindOwnedItem(name, parameters, player);

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

		private Result ExecuteCombine(string name, string? parameters, Player player)
		{
			throw new NotImplementedException();
		}

		private Result ExecuteUse(string name, string? parameters, Player player, Animate? presentAnimate)
		{
			(Item? item, Result result) = FindOwnedItem(name, parameters, player);

			if (item == null)
				return result;

			if (presentAnimate == null || item is not UsableItem usableItem || !usableItem.UsableOn.Contains(presentAnimate))
				return item.Use();

			return usableItem.UseOn(presentAnimate);
		}

		private ICollection<string>? GetTexts(TextID id)
			=> Facilities.TextsMap[this, (int)id];

		private static ICollection<string>? GetTexts(CommandID commandId, int textId)
			=> Facilities.CommandTextsMap[commandId, textId];

		private ICollection<string>? GetTexts(TextID id, string tag, string parameter)
			=> GetTexts(id).ReplaceInAll($"{{{tag}}}", parameter);

		private static ICollection<string>? GetTextsWithItem(CommandID commandID, int textId, Item item)
			=> GetTexts(commandID, textId).ReplaceInAll("{item}", item.Name);

		private (Item?, Result) FindOwnedItem(string name, string? parameters, Player player)
		{
			if (parameters == null || parameters == string.Empty)
				return (null, Result.Error(GetTexts(TextID.NoParameterGiven, "command", name)));

			(Item? item, FindResult result) = player.FindInInventory(parameters);

			return result switch
			{
				FindResult.NotFound => (null, Result.Error(GetTexts(TextID.DontOwnParameter, "param", parameters))),
				FindResult.Ambiguous => (null, Result.Error(GetTexts(TextID.ParameterAmbiguous, "param", parameters))),
				_ => (item, Result.Success())
			};
		}

		private (Item?, Result) FindPresentItem(string name, string? parameters, ICollection<Item> presentItems)
		{
			if (parameters == null || parameters == string.Empty)
				return (null, Result.Error(GetTexts(TextID.NoParameterGiven, "command", name)));

			(Item? item, FindResult result) = presentItems.ToList().FindItemByName(parameters);

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
			ParameterNotPresent
		}

		private enum PutDownTextID
		{
			CantPutDown,
			PutDown
		}
	}
}
