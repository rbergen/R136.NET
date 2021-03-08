using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using R136.Entities;
using R136.Entities.Animates;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Core
{
	public partial class Engine
	{
		private readonly List<Func<StringValues>> _turnEndingNotifiees = new();

		private TypedEntityTaskCollection? LoadEntities(string groupLabel)
		{
			if (ContextServices == null)
				return null;

			var entityReader = ContextServices.GetService<IEntityReader>();
			if (entityReader == null)
				return null;

			var entityCollection = new TypedEntityTaskCollection();
			entityCollection.Add(entityReader.ReadEntity<LayoutProperties>(groupLabel, PropertiesLabel));
			entityCollection.Add(entityReader.ReadEntity<TypedTextsMap<int>.Initializer[]>(groupLabel, TextsLabel));
			entityCollection.Add(entityReader.ReadEntity<CommandInitializer[]>(groupLabel, CommandsLabel));
			entityCollection.Add(entityReader.ReadEntity<Room.Initializer[]>(groupLabel, RoomsLabel));
			entityCollection.Add(entityReader.ReadEntity<Animate.Initializer[]>(groupLabel, AnimatesLabel));
			entityCollection.Add(entityReader.ReadEntity<Item.Initializer[]>(groupLabel, ItemsLabel));

			return entityCollection;
		}

		private async Task<bool> SetEntityGroup(string label, bool requireInitialized)
		{
			if (requireInitialized && !IsInitialized)
				return false;

			try
			{
				var entityMap = _entityTaskMap[label];
				if (entityMap == null)
					return false;

				var layoutProperties = await entityMap.Get<LayoutProperties>();

				if (layoutProperties != null)
					Facilities.Configuration.Load(layoutProperties);

				var texts = await entityMap.Get<TypedTextsMap<int>.Initializer[]>();
				if (texts != null)
					Facilities.TextsMap.LoadInitializers(texts);

				var rooms = await entityMap.Get<Room.Initializer[]>();
				if (rooms == null)
					return false;

				_rooms = Room.CreateMap(rooms);

				var animates = await entityMap.Get<Animate.Initializer[]>();
				if (animates == null)
					return false;

				_animates = Animate.UpdateOrCreateMap(_animates, animates);

				var items = await entityMap.Get<Item.Initializer[]>();
				if (items == null)
					return false;

				_items = Item.UpdateOrCreateMap(_items, items, _animates);

				var commands = await entityMap.Get<CommandInitializer[]>();
				if (commands == null)
					return false;

				_processors = CommandProcessor.UpdateOrCreateMap(_processors, commands, _items, _animates);

				if (_player == null)
					_player = new Player(_rooms, Facilities.Configuration.StartRoom);
				
				else
				{
					var snapshot = _player.TakeSnapshot();
					snapshot.Items = _items;
					snapshot.Rooms = _rooms;
					_player.RestoreSnapshot(snapshot);
				}

				SetupServices();

				return true;
			}
			catch (Exception e)
			{
				Facilities.Logger.LogDebug<Engine>($"Exception while loading entity group {label}: {e}");
				return false;
			}
		}

		private void SetupServices()
		{
			_turnEndingNotifiees.Clear();

			var serviceCollection = new ServiceCollection();
			RegisterServices(serviceCollection);
			RegisterServices(serviceCollection, _items!.Values.OfType<IGameServiceProvider>());
			RegisterServices(serviceCollection, _animates!.Values.OfType<IGameServiceProvider>());
			_processors!.RegisterServices(serviceCollection);
			_player!.RegisterServices(serviceCollection);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			Configure(serviceProvider);
			Configure(serviceProvider, _items!.Values.OfType<IGameServiceBasedConfigurator>());
			Configure(serviceProvider, _animates!.Values.OfType<IGameServiceBasedConfigurator>());
			_processors!.Configure(serviceProvider);
			_player!.Configure(serviceProvider);

			EntityBase.GameServices = serviceProvider;
		}

		private static void RegisterServices(IServiceCollection serviceCollection, IEnumerable<IGameServiceProvider> entities)
		{
			foreach (var entity in entities)
				entity.RegisterServices(serviceCollection);
		}

		private static void Configure(IServiceProvider serviceProvider, IEnumerable<IGameServiceBasedConfigurator> entities)
		{
			foreach (var entity in entities)
				entity.Configure(serviceProvider);
		}

		private bool ValidateStep(NextStep step)
		{
			if (!IsInitialized)
				throw new InvalidOperationException(EngineNotInitialized);

			return DoNext == step;
		}

		private string? GetItemLine(Room room)
		{
			var items = _items!.Values.Where(item => item.CurrentRoom == room.ID).ToArray();

			if (items.Length == 0)
				return null;

			var itemLineTexts = GetTexts(TextID.ItemLineTexts);

			if (itemLineTexts.Count < Enum.GetValues<ItemLineText>().Length)
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

			if (wayLineTexts.Count < Enum.GetValues<WayLineText>().Select(v => (int)v).Max() + 1)
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

		private bool IsInCurrentRoom(Animate animate)
			=> animate.CurrentRoom == _player!.CurrentRoom.ID;

		private bool IsAnimatePresent
			=> _animates!.Values.Any(IsInCurrentRoom);

		private ICollection<Animate> PresentAnimates
			=> _animates!.Values.Where(IsInCurrentRoom).ToArray();

		private StringValues GetTexts(TextID id)
			=> Facilities.TextsMap[this, (int)id];

		private StringValues GetTexts(TextID id, string tag, string content)
			=> GetTexts(id).ReplaceInAll($"{{{tag}}}", content);

		private Result DoPostRunProcessing(Result result)
		{
			if (!result.IsSuccess && !result.IsFailure)
				return result;

			var texts = new List<string>();

			texts.AddRangeIfNotNull(result.Message);

			foreach (var notifiee in _turnEndingNotifiees)
			{
				var notifieeTexts = notifiee.Invoke();
				if (notifieeTexts.Count > 0)
					texts.AddRange(notifieeTexts);
			}

			DoNext = _animates!.Values.Any(animate => animate.IsTriggered) ? NextStep.ProgressAnimateStatus : NextStep.ShowRoomStatus;

			return texts.Count == 0 ? result : new Result(result.Code, texts.ToArray(), result.PauseRequested);
		}

		private void PlaceAt(ItemID item, Room room)
		{
			if (_items![item].CurrentRoom == RoomID.None && !_player!.IsInPosession(item))
				_items![item].CurrentRoom = room.ID;
		}

		private void HandleBurning()
		{
			_hasTreeBurned = true;

			PlaceAt(ItemID.GreenCrystal, _rooms![Facilities.Configuration.GreenCrystalRoom]);
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
