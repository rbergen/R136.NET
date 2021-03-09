using Microsoft.Extensions.Primitives;
using R136.Entities;
using R136.Entities.CommandProcessors;
using R136.Entities.Global;
using R136.Entities.Items;
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
		public IServiceProvider? ContextServices { get; set; }

		private const string EngineNotInitialized = "Engine not initialized";
		private const string IncorrectNextStep = "Step inconsistent with DoNext";
		private const string ConfigurationLabel = "configuration";
		private const string PropertiesLabel = "properties";
		private const string CommandsLabel = "commands";
		private const string TextsLabel = "texts";
		private const string RoomsLabel = "rooms";
		private const string AnimatesLabel = "animates";
		private const string ItemsLabel = "items";
		private const string ContinuationKey = "x8KQUbtDPZwlzWT5AOeJ";

		private readonly Dictionary<string, TypedEntityTaskCollection> _entityTaskMap = new();
		private IReadOnlyDictionary<RoomID, Room>? _rooms;
		private IReadOnlyDictionary<ItemID, Item>? _items;
		private IReadOnlyDictionary<AnimateID, Animate>? _animates;
		private Player? _player;
		private CommandProcessorMap? _processors;
		private readonly List<Func<StringValues>> _turnEndingNotifiees = new();
		private bool _hasTreeBurned = false;

		public class Snapshot : Item.ISnapshotContainer, Animate.ISnapshotContainer
		{
			public Configuration? Configuration { get; set; }
			public bool HasTreeBurned { get; set; }
			public bool IsAnimateTriggered { get; set; }
			public NextStep DoNext { get; set; }
			public LocationCommandProcessor.Snapshot? LocationCommandProcessor { get; set; }
			public Item.Snapshot[]? Items { get; set; }
			public Flashlight.Snapshot? Flashlight { get; set; }
			public Animate.Snapshot[]? Animates { get; set; }
			public Player.Snapshot? Player { get; set; }
		}

		private enum TextID
		{
			NoCommand,
			InvalidCommand,
			AmbiguousCommand,
			StartMessage,
			YouAreAt,
			TooDarkToSee,
			BurnedForestDescription,
			ItemLineTexts,
			WayLineTexts,
			PlayerDead
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
