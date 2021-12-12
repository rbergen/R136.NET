using Microsoft.Extensions.DependencyInjection;
using R136.Entities.CommandProcessors;
using R136.Entities.General;
using R136.Entities.Global;
using R136.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace R136.Entities
{
	public abstract class CommandProcessor : EntityBase
	{
		public CommandProcessorID ID { get; }

		protected CommandProcessor(CommandProcessorID id)
			=> ID = id;

		public static CommandProcessorMap UpdateOrCreateMap
			(
			CommandProcessorMap? sourceMap,
			ICollection<CommandInitializer> initializers,
			IReadOnlyDictionary<ItemID, Item> items,
			IReadOnlyDictionary<AnimateID, Animate> animates
			)
		{
			LocationCommandProcessor.Snapshot? snapshot = null;

			if (sourceMap != null)
				snapshot = sourceMap.LocationProcessor.TakeSnapshot();

			var map = new CommandProcessorMap(initializers, items, animates);

			foreach (var initializer in initializers)
			{
				if (initializer.TextMap != null)
				{
					foreach (var mapping in initializer.TextMap)
						Facilities.CommandTextsMap.LoadInitializer(initializer.ID, mapping);
				}
			}

			if (snapshot != null)
				map.LocationProcessor.RestoreSnapshot(snapshot);

			return map;
		}

		public abstract Result Execute(CommandID id, string command, string? parameters, Player player);
	}

	public class CommandProcessorMap : IGameServiceProvider, IGameServiceBasedConfigurator
	{
		private readonly Dictionary<(string command, bool fullMatch), CommandID> commandIdMap;
		private readonly ItemCommandProcessor itemProcessor;
		public LocationCommandProcessor LocationProcessor { get; private set; }
		private readonly GeneralCommandProcessor generalProcessor;
		private readonly InternalCommandProcessor internalProcessor;

		public CommandProcessorMap
			(
				ICollection<CommandInitializer> initializers,
				IReadOnlyDictionary<ItemID, Item> items,
				IReadOnlyDictionary<AnimateID, Animate> animates
			)
		{
			this.commandIdMap = new Dictionary<(string, bool), CommandID>();
			this.itemProcessor = new ItemCommandProcessor(items, animates);
			LocationProcessor = new LocationCommandProcessor();
			this.generalProcessor = new GeneralCommandProcessor();
			this.internalProcessor = new InternalCommandProcessor();

			foreach (var initializer in initializers)
				this.commandIdMap[(initializer.Name, initializer.FullMatch)] = initializer.ID;
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
				CommandID.Use => this.itemProcessor,
				CommandID.Combine => this.itemProcessor,
				CommandID.Pickup => this.itemProcessor,
				CommandID.PutDown => this.itemProcessor,
				CommandID.Inspect => this.itemProcessor,
				CommandID.ConfigGet => this.internalProcessor,
				CommandID.ConfigSet => this.internalProcessor,
				CommandID.ConfigList => this.internalProcessor,
				_ => this.generalProcessor
			};

		public CommandProcessor? this[CommandProcessorID id]
			=> id switch
			{
				CommandProcessorID.General => this.generalProcessor,
				CommandProcessorID.Internal => this.internalProcessor,
				CommandProcessorID.Item => this.itemProcessor,
				CommandProcessorID.Location => LocationProcessor,
				_ => null
			};

		public (CommandProcessor? processor, CommandID? id, string? command, FindResult result) FindByName(string s)
		{
			var foundItems = this.commandIdMap.Where(pair => pair.Key.fullMatch ? pair.Key.command == s : pair.Key.command.StartsWith(s)).ToArray();

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

		public void RegisterServices(IServiceCollection serviceCollection)
			=> LocationProcessor.RegisterServices(serviceCollection);

		public void Configure(IServiceProvider serviceProvider) {}
	}

	public class CommandInitializer
	{
		public CommandID ID { get; set; }
		public string Name { get; set; } = string.Empty;

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
		public bool FullMatch { get; set; } = false;

		[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
		public Dictionary<int, string[]>? Texts { get; set; }

		[JsonIgnore]
		public IDTextMap[]? TextMap
			=> Texts?.Select(pair => new IDTextMap() { ID = pair.Key, Texts = pair.Value }).ToArray();

		public class IDTextMap : KeyedTextsMap<CommandID, int>.IInitializer
		{
			public int ID { get; set; }
			public string[] Texts { get; set; } = Array.Empty<string>();
		}
	}
}
