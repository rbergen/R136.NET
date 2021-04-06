using Microsoft.Extensions.Primitives;
using R136.Entities;
using R136.Entities.CommandProcessors;
using R136.Entities.Global;
using R136.Entities.Items;
using R136.Interfaces;
using System;
using System.Collections.Generic;

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

		public class Snapshot : Item.ISnapshotContainer, Animate.ISnapshotContainer, ISnapshot
		{
			private const int BytesBaseSize = 3;

			public Configuration? Configuration { get; set; }
			public bool HasTreeBurned { get; set; }
			public bool IsAnimateTriggered { get; set; }
			public NextStep DoNext { get; set; }

			public LocationCommandProcessor.Snapshot? LocationCommandProcessor { get; set; }
			public Item.Snapshot[]? Items { get; set; }
			public Flashlight.Snapshot? Flashlight { get; set; }
			public Animate.Snapshot[]? Animates { get; set; }
			public Player.Snapshot? Player { get; set; }

			public void AddBytes(List<byte> bytes)
			{
				HasTreeBurned.AddByte(bytes);
				IsAnimateTriggered.AddByte(bytes);
				DoNext.AddByte(bytes);

				Configuration.AddSnapshotBytes(bytes);
				LocationCommandProcessor.AddSnapshotBytes(bytes);
				Items.AddSnapshotsBytes(bytes);
				Flashlight.AddSnapshotBytes(bytes);
				Animates.AddSnapshotsBytes(bytes);
				Player.AddSnapshotBytes(bytes);
			}

			public int? LoadBytes(ReadOnlyMemory<byte> bytes)
			{
				if (bytes.Length <= BytesBaseSize)
					return null;

				var span = bytes.Span;

				HasTreeBurned = span[0].ToBool();
				IsAnimateTriggered = span[1].ToBool();
				DoNext = span[2].To<NextStep>();

				bytes = bytes[BytesBaseSize..];

				int totalBytesRead = BytesBaseSize;
				bool abort = false;

				Configuration = bytes.ToNullable<Configuration>().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
				if (abort) return null;

				LocationCommandProcessor = bytes.ToNullable<LocationCommandProcessor.Snapshot>().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
				if (abort) return null;

				Items = bytes.ToSnapshotArrayOf<Item.Snapshot>().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
				if (abort) return null;

				Flashlight = bytes.ToNullable<Flashlight.Snapshot>().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
				if (abort) return null;

				Animates = bytes.ToSnapshotArrayOf<Animate.Snapshot>().ProcessIntermediateResult(ref bytes, ref totalBytesRead, ref abort);
				if (abort) return null;

				int? bytesRead;
				(Player, bytesRead) = bytes.ToNullable<Player.Snapshot>();

				return bytesRead != null ? totalBytesRead + bytesRead.Value : null;
			}
		}

		private enum TextID : byte
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

		private enum ItemLineText : byte
		{
			SingleItem,
			MultipleItemsFormat,
			LastTwoItems,
			EarlierItem
		}

		private enum WayLineText : byte
		{
			SingleWay = 6,
			MultipleWayFormat,
			LastTwoWays,
			EarlierWay
		}
	}
}
