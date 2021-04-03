using Microsoft.Extensions.Primitives;
using R136.Entities;
using R136.Entities.CommandProcessors;
using R136.Entities.General;
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
			private const int BinaryBaseSize = 3;

			public Configuration? Configuration { get; set; }
			public bool HasTreeBurned { get; set; }
			public bool IsAnimateTriggered { get; set; }
			public NextStep DoNext { get; set; }

			public LocationCommandProcessor.Snapshot? LocationCommandProcessor { get; set; }
			public Item.Snapshot[]? Items { get; set; }
			public Flashlight.Snapshot? Flashlight { get; set; }
			public Animate.Snapshot[]? Animates { get; set; }
			public Player.Snapshot? Player { get; set; }

			public byte[] GetBinary()
			{
				List<byte> result = new();

				result.Add(HasTreeBurned.ToByte());
				result.Add(IsAnimateTriggered.ToByte());
				result.Add(DoNext.ToByte());

				result.AddRange(Configuration.ToBytes());
				result.AddRange(LocationCommandProcessor.ToBytes());
				result.AddRange(Items.SnapshotsToBytes());
				result.AddRange(Flashlight.ToBytes());
				result.AddRange(Animates.SnapshotsToBytes());
				result.AddRange(Player.ToBytes());

				return result.ToArray();
			}

			public int? SetBinary(Span<byte> value)
			{
				if (value.Length <= BinaryBaseSize)
					return null;

				HasTreeBurned = value[0].ToBool();
				IsAnimateTriggered = value[1].ToBool();
				DoNext = value[2].To<NextStep>();

				int? readBytes = BinaryBaseSize;
				int totalReadBytes = readBytes.Value;
				Memory<byte> bytes = value[readBytes.Value..].ToArray();
				bool abort = false;

				Configuration = ProcessResult(bytes.Span.ToNullable<Configuration>());
				if (abort) return null;

				LocationCommandProcessor = ProcessResult(bytes.Span.ToNullable<LocationCommandProcessor.Snapshot>());
				if (abort) return null;

				Items = ProcessResult(bytes.Span.ToSnapshotArrayOf<Item.Snapshot>());
				if (abort) return null;

				Flashlight = ProcessResult(bytes.Span.ToNullable<Flashlight.Snapshot>());
				if (abort) return null;

				Animates = ProcessResult(bytes.Span.ToSnapshotArrayOf<Animate.Snapshot>());
				if (abort) return null;

				(Player, readBytes) = bytes.Span.ToNullable<Player.Snapshot>();

				return readBytes != null ? totalReadBytes + readBytes : null;

				TResult ProcessResult<TResult>((TResult item, int? readBytes) result)
				{
					if (readBytes == null || bytes.Length == readBytes)
					{
						abort = true;
						return result.item;
					}

					bytes = bytes[readBytes.Value..];
					totalReadBytes += readBytes.Value;

					return result.item;
				}
			}
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
