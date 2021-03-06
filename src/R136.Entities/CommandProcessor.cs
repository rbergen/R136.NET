using R136.Entities.CommandProcessors;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace R136.Entities
{
	public abstract class CommandProcessor : EntityBase
	{
		public CommandProcessorID ID { get; }

		protected CommandProcessor(CommandProcessorID id)
			=> ID = id;

		public static CommandProcessorMap CreateMap
			(
			ICollection<CommandInitializer> initializers,
			IReadOnlyDictionary<ItemID, Item> items,
			IReadOnlyDictionary<AnimateID, Animate> animates
			)
		{
			var map = new CommandProcessorMap(initializers, items, animates);

			foreach (var initializer in initializers)
			{
				if (initializer.TextMap != null)
				{
					foreach (var mapping in initializer.TextMap)
						Facilities.CommandTextsMap.LoadInitializer(initializer.ID, mapping);
				}
			}

			return map;
		}

		public abstract Result Execute(CommandID id, string command, string? parameters, Player player);
	}

	public class CommandProcessorMap
	{
		private readonly Dictionary<(string command, bool fullMatch), CommandID> _commandIdMap;
		private readonly ItemCommandProcessor _itemProcessor;
		public LocationCommandProcessor LocationProcessor { get; private set; }
		private readonly GeneralCommandProcessor _generalProcessor;
		private readonly InternalCommandProcessor _internalProcessor;

		public CommandProcessorMap
			(
			ICollection<CommandInitializer> initializers,
			IReadOnlyDictionary<ItemID, Item> items,
			IReadOnlyDictionary<AnimateID, Animate> animates
			)
		{
			_commandIdMap = new Dictionary<(string, bool), CommandID>();
			_itemProcessor = new ItemCommandProcessor(items, animates);
			LocationProcessor = new LocationCommandProcessor(items, animates);
			_generalProcessor = new GeneralCommandProcessor();
			_internalProcessor = new InternalCommandProcessor();

			foreach (var initializer in initializers)
				_commandIdMap[(initializer.Name, initializer.FullMatch)] = initializer.ID;
		}

		public CommandProcessor this[CommandID id]
			=> id switch
			{
				CommandID.GoEast => LocationProcessor,
				CommandID.GoWest => LocationProcessor,
				CommandID.GoNorth => LocationProcessor,
				CommandID.GoSouth => LocationProcessor,
				CommandID.GoUp => LocationProcessor,
				CommandID.GoDown => LocationProcessor,
				CommandID.Use => _itemProcessor,
				CommandID.Combine => _itemProcessor,
				CommandID.Pickup => _itemProcessor,
				CommandID.PutDown => _itemProcessor,
				CommandID.Inspect => _itemProcessor,
				CommandID.ConfigGet => _internalProcessor,
				CommandID.ConfigSet => _internalProcessor,
				CommandID.ConfigList => _internalProcessor,
				_ => _generalProcessor
			};

		public CommandProcessor? this[CommandProcessorID id]
			=> id switch
			{
				CommandProcessorID.General => _generalProcessor,
				CommandProcessorID.Internal => _internalProcessor,
				CommandProcessorID.Item => _itemProcessor,
				CommandProcessorID.Location => LocationProcessor,
				_ => null
			};

		public (CommandProcessor? processor, CommandID? id, string? command, FindResult result) FindByName(string s)
		{
			var foundItems = _commandIdMap.Where(pair => pair.Key.fullMatch ? pair.Key.command == s : pair.Key.command.StartsWith(s)).ToArray();

			FindResult result = foundItems.Length switch
			{
				0 => FindResult.NotFound,
				1 => FindResult.Found,
				_ => FindResult.Ambiguous
			};

			return result == FindResult.Found
				? (this[foundItems[0].Value], foundItems[0].Value, foundItems[0].Key.command, result)
				: (null, null, null, result);
		}
	}

	public abstract class ActingCommandProcessor : CommandProcessor
	{
		protected IReadOnlyDictionary<ItemID, Item> Items { get; }
		protected IReadOnlyDictionary<AnimateID, Animate> Animates { get; }

		public ActingCommandProcessor(CommandProcessorID id, IReadOnlyDictionary<ItemID, Item> items, IReadOnlyDictionary<AnimateID, Animate> animates) : base(id)
			=> (Items, Animates) = (items, animates);
	}

	public class CommandInitializer
	{
		public CommandID ID { get; set; }
		public string Name { get; set; } = "";
		public bool FullMatch { get; set; } = false;
		public IDTextMap[]? TextMap { get; set; }

		public class IDTextMap : KeyedTextsMap<CommandID, int>.IInitializer
		{
			public int ID { get; set; }
			public string[] Texts { get; set; } = Array.Empty<string>();
		}
	}
}
