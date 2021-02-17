using R136.Entities.CommandProcessors;
using R136.Entities.General;
using R136.Entities.Global;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace R136.Entities
{
	abstract class CommandProcessor : EntityBase {
		public static CommandProcessorMap FromInitializers(ICollection<CommandInitializer> initializers, IReadOnlyDictionary<ItemID, Item> items, 
			IReadOnlyCollection<INotifyRoomChangeRequested> roomChangeNotifiees, ITriggerable? paperrouteTriggerable)
		{
			var map = new CommandProcessorMap(initializers, items, roomChangeNotifiees, paperrouteTriggerable);

			foreach(var initializer in initializers)
			{
				if (initializer.TextMap != null)
				{
					foreach(var mapping in initializer.TextMap)
						Facilities.CommandTextsMap[initializer.ID, mapping.ID] = mapping.Texts;
				}
			}

			return map;
		}

		public abstract Result Execute(CommandID id, string name, string? parameters, Player player, ICollection<Item> presentItems, Animate? presentAnimate);
	}

	class CommandProcessorMap 
	{
		private readonly Dictionary<string, CommandID> _nameIdMap;
		private readonly ItemCommandProcessor _itemProcessor;
		private readonly LocationCommandProcessor _locationProcessor;
		private readonly GeneralCommandProcessor _generalProcessor;

		public CommandProcessorMap(ICollection<CommandInitializer> initializers, IReadOnlyDictionary<ItemID, Item> items, 
			IReadOnlyCollection<INotifyRoomChangeRequested> roomChangeNotifiees, ITriggerable? paperrouteTriggerable)
		{
			_nameIdMap = new Dictionary<string, CommandID>();
			_itemProcessor = new ItemCommandProcessor(items);
			_locationProcessor = new LocationCommandProcessor(roomChangeNotifiees, paperrouteTriggerable);
			_generalProcessor = new GeneralCommandProcessor();

			foreach (var initializer in initializers)
				_nameIdMap[initializer.Name] = initializer.ID;
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
				_ => _generalProcessor
			};
			
		public (string? name, CommandProcessor? processor, FindResult result) FindByName(string s)
		{
			var foundItems = _nameIdMap.Where(pair => pair.Key.Contains(s)).ToArray();

			FindResult result = foundItems.Length switch
			{
				0 => FindResult.NotFound,
				1 => FindResult.Found,
				_ => FindResult.Ambiguous
			};

			return result == FindResult.Found 
				? (foundItems[0].Key, this[foundItems[0].Value], result)
				: (null, null, result);
		}
	}
	
	class CommandInitializer
	{
		public CommandID ID { get; set; }
		public string Name { get; set; } = "";
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
		Help
	}
}
