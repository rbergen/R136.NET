using R136.Entities;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Entities.Items;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace R136.Core
{
	public class Engine : IEngine, IStatusManager
	{
		public IServiceProvider? ServiceProvider { get; set; }
		public IStatusManager StatusManager => this;
		public bool Initialized { get; private set; } = false;

		private const string EngineNotInitialized = "Engine not initialized";
		private const string ConfigurationLabel = "configuration";
		private const string CommandsLabel = "commands";
		private const string TextsLabel = "texts";
		private const string RoomsLabel = "rooms";
		private const string AnimatesLabel = "animates";
		private const string ItemsLabel = "items";

		private IReadOnlyDictionary<RoomID, Room>? _rooms;
		private IReadOnlyDictionary<ItemID, Item>? _items;
		private IReadOnlyDictionary<AnimateID, Animate>? _animates;
		private Player? _player;
		private CommandProcessorMap? _processors;
		private object _run = new object();

		public int LifePoints 
			=> Initialized ? _player!.LifePoints : throw new InvalidOperationException(EngineNotInitialized);

		public RoomID CurrentRoom
		{
			get => Initialized ? _player!.CurrentRoom.ID : throw new InvalidOperationException(EngineNotInitialized);
			set
			{
				if (!Initialized)
					throw new InvalidOperationException(EngineNotInitialized);

				_player!.CurrentRoom = _rooms![value];
			}
		}

		public bool IsDark
		{
			get
			{
				if (!Initialized)
					throw new InvalidOperationException(EngineNotInitialized);

				return _player!.CurrentRoom.IsDark && (!IsInPosession(ItemID.Flashlight) || !((Flashlight)_items![ItemID.Flashlight]).IsOn);
			}
		}

		public Engine()
		{
			
		}

		public async Task<bool> Initialize()
		{
			if (Initialized)
				return true;

			if (ServiceProvider == null)
				return false;

			Facilities.ServiceProvider = ServiceProvider;

			var entityReader = (IEntityReader?)ServiceProvider.GetService(typeof(IEntityReader));
			if (entityReader == null)
				return false;

			var configurationTask = entityReader.ReadEntity<Configuration>(ConfigurationLabel);
			var textsTask = entityReader.ReadEntity<TypedTextsMap<int>.Initializer[]>(TextsLabel);
			var commandsTask = entityReader.ReadEntity<CommandInitializer[]>(CommandsLabel);
			var roomsTask = entityReader.ReadEntity<Room.Initializer[]>(RoomsLabel);
			var animatesTask = entityReader.ReadEntity<Animate.Initializer[]>(AnimatesLabel);
			var itemsTask = entityReader.ReadEntity<Item.Initializer[]>(ItemsLabel);

			var configuration = await configurationTask;
			if (configuration != null)
				Facilities.Configuration = configuration;

			if (Facilities.TextsMap.TextsMapCount == 0)
				Facilities.TextsMap.LoadInitializers(await textsTask);

			if (_rooms == null) {
				var rooms = await roomsTask;
				if (rooms == null)
					return false;
			}

			_rooms = Room.FromInitializers(rooms);

			var animates = await animatesTask;
			if (animates == null)
				return false;

			_animates = Animate.FromInitializers(animates);

			var items = await animatesTask;
			if (items == null)
				return false;

			_animates = Animate.FromInitializers(animates);

			var commands = await commandsTask;
			if (commands == null)
				return false;

			_processors = CommandProcessor.FromInitializers()
		}

		public Result Continue(object statusData, string input)
		{ 
			
		}

		public Result Run(string input)
			=> Continue(_run, input);

		public void DecreaseHealth()
		{
			if (!Initialized)
				throw new InvalidOperationException(EngineNotInitialized);

			_player!.DecreaseHealth();
		}

		public void DecreaseHealth(HealthImpact impact)
		{
			if (!Initialized)
				throw new InvalidOperationException(EngineNotInitialized);

			_player!.DecreaseHealth(impact);
		}

		public void RestoreHealth()
		{
			if (!Initialized)
				throw new InvalidOperationException(EngineNotInitialized);

			_player!.RestoreHealth();
		}

		public bool IsInPosession(ItemID item)
			=> Initialized ? _player!.FindInInventory(item) != null : throw new InvalidOperationException(EngineNotInitialized);

		public void RemoveFromPossession(ItemID item)
		{
			if (!Initialized)
				throw new InvalidOperationException(EngineNotInitialized);

			_player!.RemoveFromInventory(item);
		}

		public void PutDown(ItemID item)
		{
			if (!Initialized)
				throw new InvalidOperationException(EngineNotInitialized);

			_items![item].CurrentRoom = CurrentRoom;
		}

		public void OpenConnection(Direction direction, RoomID toRoom) {
			if (!Initialized)
				throw new InvalidOperationException(EngineNotInitialized);

			_rooms![CurrentRoom].Connections[direction] = _rooms![toRoom];
		}
	}

}
