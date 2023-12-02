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
		private int? lifePoints;
		private int? lifePointsFromConfig;
		private ILightsource? lightsource;
		private IReadOnlyDictionary<RoomID, Room> rooms;
		private readonly List<Item> inventory;

		public Room CurrentRoom { get; set; }

		public IReadOnlyList<Item> Inventory => this.inventory;

		public Player(IReadOnlyDictionary<RoomID, Room> rooms, RoomID startRoom)
			=> (this.lifePoints, this.lifePointsFromConfig, this.rooms, CurrentRoom, this.inventory) = (null, null, rooms, rooms[startRoom], new List<Item>());

		public int LifePoints
		{
			get
			{
				if (this.lifePointsFromConfig != Facilities.Configuration.LifePoints)
					this.lifePoints = this.lifePointsFromConfig = Facilities.Configuration.LifePoints;

				return this.lifePoints!.Value;
			}

			private set => this.lifePoints = value;
		}

		public bool IsDark
			=> CurrentRoom.IsDark && !(this.lightsource?.IsOn ?? false);

		RoomID IPlayer.CurrentRoom 
		{
			get => CurrentRoom.ID; 
			set => CurrentRoom = this.rooms[value]; 
		}

		private StringValues GetNamedTexts(TextID id, Item item) => Facilities.TextsMap.Get(this, id).ReplaceInAll("{item}", item.Name);

		public Result AddToInventory(Item item)
		{
			if (Facilities.Configuration.MaxInventory != null && this.inventory.Where(item => !item.IsWearable).ToArray().Length >= Facilities.Configuration.MaxInventory)
				return Result.Failure(GetNamedTexts(TextID.InventoryFull, item));

			this.inventory.Add(item);

			return Result.Success(GetNamedTexts(item.IsWearable ? TextID.StartedWearing : TextID.AddedToInventory, item));
		}

		public Item? FindInInventory(ItemID id)
			=> this.inventory.FirstOrDefault(item => item.ID == id);

		public bool RemoveFromInventory(Item item)
			=> this.inventory.Remove(item);

		public bool RemoveFromInventory(ItemID id)
			=> this.inventory.RemoveAll(item => item.ID == id) > 0;

		public (Item? item, FindResult result) FindInInventory(string s)
			=> this.inventory.FindItemByName(s);

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
			if (this.lifePointsFromConfig != Facilities.Configuration.LifePoints)
				this.lifePointsFromConfig = Facilities.Configuration.LifePoints;

			this.lifePoints = this.lifePointsFromConfig;
		}

		public Snapshot TakeSnapshot(Snapshot? snapshot = null)
		{
			if (snapshot == null)
				snapshot = new();

			snapshot.LifePoints = this.lifePoints;
			snapshot.LifePointsFromConfig = this.lifePointsFromConfig;
			snapshot.Room = CurrentRoom.ID;
			snapshot.Inventory = this.inventory.Select(item => item.ID).ToArray();

			return snapshot;
		}

		public bool RestoreSnapshot(Snapshot snapshot)
		{
			if (snapshot.Rooms == null || snapshot.Items == null)
				return false;

			this.rooms = snapshot.Rooms;
			this.lifePoints = snapshot.LifePoints;
			this.lifePointsFromConfig = snapshot.LifePointsFromConfig;
			CurrentRoom = this.rooms[snapshot.Room];

			this.inventory.Clear();
			if (snapshot.Inventory != null)
				this.inventory.AddRange(snapshot.Inventory.Select(itemId => snapshot.Items[itemId]));

			return true;
		}

		public bool IsInPossession(ItemID item)
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
			=> this.lightsource = serviceProvider.GetService<ILightsource>();

		public class Snapshot : IRoomsReader, IItemsReader, ISnapshot
		{
			private const int BytesBaseSize = 1;

			public int? LifePoints { get; set; }
			public int? LifePointsFromConfig { get; set; }
			public RoomID Room { get; set; }
			public ItemID[]? Inventory { get; set; }

			[JsonIgnore]
			public IReadOnlyDictionary<ItemID, Item>? Items { get; set; }

			[JsonIgnore]
			public IReadOnlyDictionary<RoomID, Room>? Rooms { get; set; }

			public void AddBytesTo(List<byte> bytes)
			{
				Room.AddByteTo(bytes);
				LifePoints.AddIntBytesTo(bytes);
				LifePointsFromConfig.AddIntBytesTo(bytes);
				Inventory.AddEnumsBytesTo(bytes);
			}

			public int? LoadBytes(ReadOnlyMemory<byte> bytes)
			{
				if (bytes.Length <= BytesBaseSize)
					return null;

				Room = bytes.Span[0].To<RoomID>();

				bytes = bytes[BytesBaseSize..];

				int totalBytesRead = BytesBaseSize;
				bool abort = false;

				LifePoints = bytes.ToNullableInt().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
				if (abort) return null;

				LifePointsFromConfig = bytes.ToNullableInt().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
				if (abort) return null;

				int? bytesRead;
				(Inventory, bytesRead) = bytes.ToEnumArrayOf<ItemID>();

				return bytesRead != null ? totalBytesRead + bytesRead.Value : null;
			}
		}

		private enum TextID : byte
		{
			InventoryFull,
			AddedToInventory,
			StartedWearing
		}
	}
}
