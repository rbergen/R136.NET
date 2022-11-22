using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using R136.Entities;
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
				var entityMap = this.entityTaskMap[label];
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

				this.rooms = Room.CreateMap(rooms);

				var animates = await entityMap.Get<Animate.Initializer[]>();
				if (animates == null)
					return false;

				this.animates = Animate.UpdateOrCreateMap(this.animates, animates);

				var items = await entityMap.Get<Item.Initializer[]>();
				if (items == null)
					return false;

				this.items = Item.UpdateOrCreateMap(this.items, items, this.animates);

				var commands = await entityMap.Get<CommandInitializer[]>();
				if (commands == null)
					return false;

				this.processors = CommandProcessor.UpdateOrCreateMap(this.processors, commands, this.items, this.animates);

				if (this.player == null)
					this.player = new Player(this.rooms, Facilities.Configuration.StartRoom);
				
				else
				{
					var snapshot = this.player.TakeSnapshot();
					snapshot.Items = this.items;
					snapshot.Rooms = this.rooms;
					this.player.RestoreSnapshot(snapshot);
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
			this.turnEndingNotifiees.Clear();

			var serviceCollection = new ServiceCollection();
			RegisterServices(serviceCollection);
			RegisterServices(serviceCollection, this.items!.Values.OfType<IGameServiceProvider>());
			RegisterServices(serviceCollection, this.animates!.Values.OfType<IGameServiceProvider>());
			this.processors!.RegisterServices(serviceCollection);
			this.player!.RegisterServices(serviceCollection);

			var serviceProvider = serviceCollection.BuildServiceProvider();
			Configure(serviceProvider);
			Configure(serviceProvider, this.items!.Values.OfType<IGameServiceBasedConfigurator>());
			Configure(serviceProvider, this.animates!.Values.OfType<IGameServiceBasedConfigurator>());
			this.processors!.Configure(serviceProvider);
			this.player!.Configure(serviceProvider);

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

		private void AddRoomInformation(List<string> texts, Room playerRoom)
		{
			texts.AddRangeIfNotNull(GetTexts(TextID.YouAreAt, "room", playerRoom.Name));

			if (this.player!.IsDark)
				texts.AddRangeIfNotNull(GetTexts(TextID.TooDarkToSee));

			else
			{
				if (playerRoom.IsForest && this.hasTreeBurned)
					texts.AddRangeIfNotNull(GetTexts(TextID.BurnedForestDescription));

				else if (playerRoom.Description != null)
					texts.Add(playerRoom.Description);
			}
		}

		private string? GetItemLine(Room room)
		{
			var items = this.items!.Values.Where(item => item.CurrentRoom == room.ID).ToArray();

			if (items.Length == 0)
				return null;

			var itemLineTexts = GetTexts(TextID.ItemLineTexts);

			if (itemLineTexts.Count < Enum.GetValues<ItemLineText>().Length)
				return null;

			var itemLineTextList = itemLineTexts.ToArray();

			if (items.Length == 1)
				return itemLineTextList.Get(ItemLineText.SingleItem).Replace("{item}", items[0].Name);

			string itemSection = itemLineTextList.Get(ItemLineText.LastTwoItems).ReplacePlaceholders(new Dictionary<string, object>
			{
				{ "firstitem", items[^2].Name },
				{ "seconditem", items[^1].Name }
			});

			if (items.Length > 2)
			{
				StringBuilder itemSectionBuilder = new();
				foreach (var component in items[..^2].Select(item => itemLineTextList.Get(ItemLineText.EarlierItem).Replace("{item}", item.Name)))
					itemSectionBuilder.Append(component);

				itemSectionBuilder.Append(itemSection);
				itemSection = itemSectionBuilder.ToString();
			}

			return itemLineTextList.Get(ItemLineText.MultipleItemsFormat).Replace("{items}", itemSection);
		}

		private string? GetWayLine(Room room)
		{
			var ways = room.Connections.Keys.ToArray();

			if (ways.Length == 0)
				return null;

			var wayLineTexts = GetTexts(TextID.WayLineTexts);

			if (wayLineTexts.Count < Enum.GetValues<WayLineText>().Max(v => Convert.ToInt32(v)) + 1)
				return null;

			var wayLineTextList = wayLineTexts.ToArray();

			if (ways.Length == 1)
				return wayLineTextList.Get(WayLineText.SingleWay).Replace("{way}", wayLineTextList.Get(ways[0]));

			string waySection = wayLineTextList.Get(WayLineText.LastTwoWays).ReplacePlaceholders(new Dictionary<string, object>
			{
				{ "firstway", wayLineTextList.Get(ways[^2]) },
				{ "secondway", wayLineTextList.Get(ways[^1]) }
			});

			if (ways.Length > 2)
			{
				StringBuilder waySectionBuilder = new();
				foreach (var component in ways[..^2].Select(way => wayLineTextList.Get(WayLineText.EarlierWay).Replace("{way}", wayLineTextList.Get(way))))
					waySectionBuilder.Append(component);

				waySectionBuilder.Append(waySection);
				waySection = waySectionBuilder.ToString();
			}

			return wayLineTextList.Get(WayLineText.MultipleWayFormat).Replace("{ways}", waySection);
		}

		private static (List<string> texts, bool isAnimateTriggered) ProgressPresentAnimateStatuses(ICollection<Animate> presentAnimates)
		{
			var texts = new List<string>();
			bool isAnimateTriggered = false;

			foreach (var animate in presentAnimates)
			{
				if (texts.Count > 0)
					texts.Add(string.Empty);

				texts.AddRangeIfNotNull(animate.ProgressStatus());

				if (animate.IsTriggered)
				{
					isAnimateTriggered = true;
					animate.ResetTrigger();
				}
			}

			return (texts, isAnimateTriggered);
		}

		private Result DoPostRunProcessing(Result result)
		{
			if (!result.IsSuccess && !result.IsFailure)
				return result;

			var texts = new List<string>();

			texts.AddRangeIfNotNull(result.Message);

			foreach (var notifiee in this.turnEndingNotifiees)
			{
				var notifieeTexts = notifiee.Invoke();
				if (notifieeTexts.Count > 0)
					texts.AddRange(notifieeTexts);
			}

			DoNext = PresentAnimates.Any(animate => animate.IsTriggered) ? NextStep.ProgressAnimateStatus : NextStep.ShowRoomStatus;

			return texts.Count == 0 ? result : new Result(result.Code, texts.ToArray(), result.PauseRequested);
		}

		private bool IsInCurrentRoom(Animate animate)
			=> animate.CurrentRoom == this.player!.CurrentRoom.ID;

		private bool IsAnimatePresent
			=> this.animates!.Values.Any(IsInCurrentRoom);

		private ICollection<Animate> PresentAnimates
			=> this.animates!.Values.Where(IsInCurrentRoom).ToArray();

		private StringValues GetTexts(TextID id)
			=> Facilities.TextsMap.Get(this, id);

		private StringValues GetTexts(TextID id, string tag, string content)
			=> GetTexts(id).ReplaceInAll($"{{{tag}}}", content);

		private void PlaceAt(ItemID item, Room room)
		{
			if (this.items![item].CurrentRoom == RoomID.None && !this.player!.IsInPossession(item))
				this.items![item].CurrentRoom = room.ID;
		}

		private TSnapshot AddEntities<TSnapshot>(TSnapshot snapshot)
		{
			if (snapshot is IRoomsReader roomsReader)
				roomsReader.Rooms = this.rooms;

			if (snapshot is IItemsReader itemsReader)
				itemsReader.Items = this.items;

			return snapshot;
		}

		private void HandleBurning()
		{
			this.hasTreeBurned = true;

			PlaceAt(ItemID.GreenCrystal, this.rooms![Facilities.Configuration.GreenCrystalRoom]);
		}
	}
}
