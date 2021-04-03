using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace R136.Entities
{
	public class Player : EntityBase, IPlayer, IGameServiceProvider, IGameServiceBasedConfigurator, ISnappable<Player.Snapshot>
	{
		private int? _lifePoints;
		private int? _lifePointsFromConfig;
		private ILightsource? _lightsource;
		private IReadOnlyDictionary<RoomID, Room> _rooms;

		public Room CurrentRoom { get; set; }

		public IReadOnlyList<Item> Inventory => _inventory;

		private readonly List<Item> _inventory;

		public Player(IReadOnlyDictionary<RoomID, Room> rooms, RoomID startRoom)
			=> (_lifePoints, _lifePointsFromConfig, _rooms, CurrentRoom, _inventory) = (null, null, rooms, rooms[startRoom], new List<Item>());

		public int LifePoints
		{
			get
			{
				if (_lifePointsFromConfig != Facilities.Configuration.LifePoints)
					_lifePoints = _lifePointsFromConfig = Facilities.Configuration.LifePoints;

				return _lifePoints!.Value;
			}

			private set => _lifePoints = value;
		}

		public bool IsDark
			=> CurrentRoom.IsDark && !(_lightsource?.IsOn ?? false);

		RoomID IPlayer.CurrentRoom 
		{
			get => CurrentRoom.ID; 
			set => CurrentRoom = _rooms[value]; 
		}

		private StringValues GetNamedTexts(TextID id, Item item) => Facilities.TextsMap.Get(this, id).ReplaceInAll("{item}", item.Name);

		public Result AddToInventory(Item item)
		{
			if (Facilities.Configuration.MaxInventory != null && _inventory.Where(item => !item.IsWearable).ToArray().Length >= Facilities.Configuration.MaxInventory)
				return Result.Failure(GetNamedTexts(TextID.InventoryFull, item));

			_inventory.Add(item);

			return Result.Success(GetNamedTexts(item.IsWearable ? TextID.StartedWearing : TextID.AddedToInventory, item));
		}

		public Item? FindInInventory(ItemID id)
			=> _inventory.FirstOrDefault(item => item.ID == id);

		public bool RemoveFromInventory(Item item)
			=> _inventory.Remove(item);

		public bool RemoveFromInventory(ItemID id)
			=> _inventory.RemoveAll(item => item.ID == id) > 0;

		public (Item? item, FindResult result) FindInInventory(string s)
			=> _inventory.FindItemByName(s);

		public void DecreaseHealth()
			=> DecreaseHealth(HealthImpact.Normal);

		public void DecreaseHealth(HealthImpact impact)
		{
			if (Facilities.Configuration.Immortal)
				return;

			LifePoints -= Convert.ToInt32(impact);

			if (LifePoints < 0)
				LifePoints = 0;
		}

		public bool IsDead
			=> LifePoints == 0;

		public void RestoreHealth()
		{
			if (_lifePointsFromConfig != Facilities.Configuration.LifePoints)
				_lifePointsFromConfig = Facilities.Configuration.LifePoints;

			_lifePoints = _lifePointsFromConfig;
		}

		public Snapshot TakeSnapshot(Snapshot? snapshot = null)
		{
			if (snapshot == null)
				snapshot = new();

			snapshot.LifePoints = _lifePoints;
			snapshot.LifePointsFromConfig = _lifePointsFromConfig;
			snapshot.Room = CurrentRoom.ID;
			snapshot.Inventory = _inventory.Select(item => item.ID).ToArray();

			return snapshot;
		}

		public bool RestoreSnapshot(Snapshot snapshot)
		{
			if (snapshot.Rooms == null || snapshot.Items == null)
				return false;

			_rooms = snapshot.Rooms;
			_lifePoints = snapshot.LifePoints;
			_lifePointsFromConfig = snapshot.LifePointsFromConfig;
			CurrentRoom = _rooms[snapshot.Room];

			_inventory.Clear();
			if (snapshot.Inventory != null)
				_inventory.AddRange(snapshot.Inventory.Select(itemId => snapshot.Items[itemId]));

			return true;
		}

		public bool IsInPosession(ItemID item)
			=> FindInInventory(item) != null;

		public void RemoveFromPossession(ItemID id)
		{
			var item = FindInInventory(id);

			if (item != null)
				RemoveFromInventory(item);
		}

		public void RegisterServices(IServiceCollection serviceCollection)
			=> serviceCollection.AddSingleton<IPlayer>(this);

		public void Configure(IServiceProvider serviceProvider)
			=> _lightsource = serviceProvider.GetService<ILightsource>();

		public class Snapshot : IRoomsReader, IItemsReader
		{
			public int ID { get; set; }
			public int? LifePoints { get; set; }
			public int? LifePointsFromConfig { get; set; }
			public RoomID Room { get; set; }
			public ItemID[]? Inventory { get; set; }

			[JsonIgnore]
			public IReadOnlyDictionary<ItemID, Item>? Items { get; set; }

			[JsonIgnore]
			public IReadOnlyDictionary<RoomID, Room>? Rooms { get; set; }
		}

		private enum TextID
		{
			InventoryFull,
			AddedToInventory,
			StartedWearing
		}
	}
}
