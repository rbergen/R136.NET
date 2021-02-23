using R136.Entities;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace R136.Core
{
	public partial class Engine
	{
		private readonly List<Func<ICollection<string>?>> _turnEndingNotifiees = new List<Func<ICollection<string>?>>();

		private void RegisterTurnEndingNotifiees<TEntity>(IEnumerable<TEntity> entities)
		{
			foreach (var notifiee in entities.Where(entity => entity is INotifyTurnEnding).Cast<INotifyTurnEnding>())
				_turnEndingNotifiees.Add(notifiee.TurnEnding);
		}
		private void LogLine(string text)
		{
			Facilities.LogLine(this, text);
		}

		private bool ValidateStep(NextStep step)
		{
			if (!IsInitialized && !Initialize().Result)
				throw new InvalidOperationException(EngineNotInitialized);

			return DoNext == step;
		}

		private string? GetItemLine(Room room)
		{
			var items = _items!.Values.Where(item => item.CurrentRoom == room.ID).ToArray();

			if (items.Length == 0)
				return null;

			var itemLineTexts = GetTexts(TextID.ItemLineTexts);

			if (itemLineTexts == null || itemLineTexts.Count < Enum.GetValues<ItemLineText>().Length)
				return null;

			var itemLineList = itemLineTexts.ToArray();

			if (items.Length == 1)
				return itemLineList[(int)ItemLineText.SingleItem].Replace("{item}", items[0].Name);

			var itemSection = itemLineList[(int)ItemLineText.LastTwoItems].Replace("{seconditem}", items[^1].Name).Replace("{firstitem}", items[^2].Name);

			if (items.Length > 2)
			{
				var itemSectionBuilder = new StringBuilder();
				foreach (var component in items[..^2].Select(item => itemLineList[(int)ItemLineText.EarlierItem].Replace("{item}", item.Name)))
					itemSectionBuilder.Append(component);

				itemSectionBuilder.Append(itemSection);
				itemSection = itemSectionBuilder.ToString();
			}

			return itemLineList[(int)ItemLineText.MultipleItemsFormat].Replace("{items}", itemSection);
		}

		private string? GetWayLine(Room room)
		{
			var ways = room.Connections.Keys.ToArray();

			if (ways.Length == 0)
				return null;

			var wayLineTexts = GetTexts(TextID.WayLineTexts);

			if (wayLineTexts == null || wayLineTexts.Count < Enum.GetValues<WayLineText>().Select(v => (int)v).Max() + 1)
				return null;

			var wayLineList = wayLineTexts.ToArray();

			if (ways.Length == 1)
				return wayLineList[(int)WayLineText.SingleWay].Replace("{way}", wayLineList[(int)ways[0]]);

			var waySection = wayLineList[(int)WayLineText.LastTwoWays].Replace("{secondway}", wayLineList[(int)ways[^1]]).Replace("{firstway}", wayLineList[(int)ways[^2]]);

			if (ways.Length > 2)
			{
				var waySectionBuilder = new StringBuilder();
				foreach (var component in ways[..^2].Select(way => wayLineList[(int)WayLineText.EarlierWay].Replace("{way}", wayLineList[(int)way])))
					waySectionBuilder.Append(component);

				waySectionBuilder.Append(waySection);
				waySection = waySectionBuilder.ToString();
			}

			return wayLineList[(int)WayLineText.MultipleWayFormat].Replace("{ways}", waySection);
		}

		private bool IsAnimatePresent
			=> _animates!.Values.Any(animate => animate.CurrentRoom == CurrentRoom);

		private ICollection<Animate> PresentAnimates
			=> _animates!.Values.Where(animate => animate.CurrentRoom == CurrentRoom).ToArray();

		private ICollection<string>? GetTexts(TextID id)
			=> Facilities.TextsMap[this, (int)id];

		private ICollection<string>? GetTexts(TextID id, string tag, string content)
			=> GetTexts(id).ReplaceInAll($"{{{tag}}}", content);

		private Result DoPostRunProcessing(Result result)
		{
			if (!result.IsSuccess && !result.IsFailure)
				return result;

			List<string> texts = new List<string>();

			texts.AddRangeIfNotNull(result.Message);

			foreach (var notifiee in _turnEndingNotifiees)
			{
				var notifieeTexts = notifiee.Invoke();
				if (notifieeTexts != null && notifieeTexts.Count > 0)
					texts.AddRange(notifieeTexts);
			}

			DoNext = _isAnimateTriggered ? NextStep.ProgressAnimateStatus : NextStep.ShowRoomStatus;

			return texts.Count == 0 ? result : new Result(result.Code, texts);
		}

		private void PlaceAt(ItemID item, RoomID room)
		{
			if (_items![item].CurrentRoom == RoomID.None && !IsInPosession(item))
				_items![item].CurrentRoom = room;
		}
		private void TreeHasBurned()
		{
			_hasTreeBurned = true;

			if (_animates![AnimateID.GreenCrystal] is ITriggerable greenCrystal)
				greenCrystal.Trigger();

			PlaceAt(ItemID.GreenCrystal, RoomID.Forest4);
		}

		private enum ItemLineText
		{
			SingleItem,
			MultipleItemsFormat,
			LastTwoItems,
			EarlierItem
		}

		private enum WayLineText
		{
			SingleWay = 6,
			MultipleWayFormat,
			LastTwoWays,
			EarlierWay
		}
	}
}
