using R136.Entities.CommandProcessors;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities
{
	public abstract class CommandProcessor : EntityBase
	{
		public static CommandProcessorMap FromInitializers
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
						Facilities.CommandTextsMap[initializer.ID, mapping.ID] = mapping.Texts;
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
		private readonly LocationCommandProcessor _locationProcessor;
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
			_locationProcessor = new LocationCommandProcessor(items, animates);
			_generalProcessor = new GeneralCommandProcessor();
			_internalProcessor = new InternalCommandProcessor();

			foreach (var initializer in initializers)
				_commandIdMap[(initializer.Name, initializer.FullMatch)] = initializer.ID;
		}

		public CommandProcessor this[CommandID id]
			=> id switch
			{
				CommandID.GoEast => _locationProcessor,
				CommandID.GoWest => _locationProcessor,
				CommandID.GoNorth => _locationProcessor,
				CommandID.GoSouth => _locationProcessor,
				CommandID.Use => _itemProcessor,
				CommandID.Combine => _itemProcessor,
				CommandID.Pickup => _itemProcessor,
				CommandID.PutDown => _itemProcessor,
				CommandID.Inspect => _itemProcessor,
				CommandID.ConfigGet => _internalProcessor,
				CommandID.ConfigSet => _internalProcessor,
				_ => _generalProcessor
			};

		public (string? command, CommandProcessor? processor, FindResult result) FindByName(string s)
		{
			var foundItems = _commandIdMap.Where(pair => pair.Key.fullMatch ? pair.Key.command == s : pair.Key.command.StartsWith(s)).ToArray();

			FindResult result = foundItems.Length switch
			{
				0 => FindResult.NotFound,
				1 => FindResult.Found,
				_ => FindResult.Ambiguous
			};

			return result == FindResult.Found
				? (foundItems[0].Key.command, this[foundItems[0].Value], result)
				: (null, null, result);
		}
	}

	abstract class ActingCommandProcessor : CommandProcessor
	{
		protected IReadOnlyDictionary<ItemID, Item> Items { get; }
		protected IReadOnlyDictionary<AnimateID, Animate> Animates { get; }

		public ActingCommandProcessor(IReadOnlyDictionary<ItemID, Item> items, IReadOnlyDictionary<AnimateID, Animate> animates)
			=> (Items, Animates) = (items, animates);
	}

	public class CommandInitializer
	{
		public CommandID ID { get; set; }
		public string Name { get; set; } = "";
		public bool FullMatch { get; set; } = false;
		public IDTextMap[]? TextMap { get; set; }

		public class IDTextMap
		{
			public int ID { get; set; }
			public string[] Texts { get; set; } = Array.Empty<string>();
		}
	}

	public enum CommandID
	{
		GoEast,
		GoWest,
		GoNorth,
		GoSouth,
		GoUp,
		GoDown,
		Use,
		Combine,
		Pickup,
		PutDown,
		Inspect,
		Wait,
		End,
		Status,
		Help,
		ConfigGet,
		ConfigSet
	}
}
