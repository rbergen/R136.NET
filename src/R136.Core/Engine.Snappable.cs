using R136.Entities;
using R136.Entities.CommandProcessors;
using R136.Entities.Global;
using R136.Entities.Items;
using R136.Interfaces;
using System;
using System.Linq;

namespace R136.Core
{
	public partial class Engine : ISnappable<Engine.Snapshot>
	{
		public Snapshot TakeSnapshot(Snapshot? snapshot = null)
		{
			if (!IsInitialized)
				throw new InvalidOperationException(EngineNotInitialized);

			if (snapshot == null)
				snapshot = new Snapshot();

			snapshot.Configuration = Facilities.Configuration;
			snapshot.HasTreeBurned = _hasTreeBurned;
			snapshot.DoNext = DoNext;
			snapshot.LocationCommandProcessor = _processors!.LocationProcessor.TakeSnapshot();
			snapshot.Items = _items!.Values
												.Where(item => item is not Flashlight)
												.Select(item => item.TakeSnapshot())
												.ToArray();
			snapshot.Flashlight = ((Flashlight)_items![ItemID.Flashlight]).TakeSnapshot();
			snapshot.Animates = _animates!.Values
													.Select(animate => animate.TakeSnapshot())
													.ToArray();
			snapshot.Player = _player!.TakeSnapshot();

			return snapshot;
		}

		private TSnapshot AddEntities<TSnapshot>(TSnapshot snapshot)
		{
			if (snapshot is IRoomsReader roomsReader)
				roomsReader.Rooms = _rooms;

			if (snapshot is IItemsReader itemsReader)
				itemsReader.Items = _items;

			return snapshot;
		}

		public bool RestoreSnapshot(Snapshot snapshot)
		{
			if (!IsInitialized)
				throw new InvalidOperationException(EngineNotInitialized);

			bool result = true;

			if (snapshot.Configuration != null)
				Facilities.Configuration.Load(snapshot.Configuration);

			_hasTreeBurned = snapshot.HasTreeBurned;
			DoNext = snapshot.DoNext;

			if (snapshot.LocationCommandProcessor != null)
				result &= _processors!.LocationProcessor.RestoreSnapshot(AddEntities(snapshot.LocationCommandProcessor));
			else
				result = false;

			if (snapshot.Items != null)
			{
				foreach (var item in snapshot.Items)
					result &= _items![item.ID].RestoreSnapshot(AddEntities(item));
			}
			else
				result = false;

			if (snapshot.Flashlight != null)
				result &= ((Flashlight)_items![ItemID.Flashlight]).RestoreSnapshot(AddEntities(snapshot.Flashlight));
			else
				result = false;

			if (snapshot.Animates != null)
			{
				foreach (var animate in snapshot.Animates)
					result &= _animates![animate.ID].RestoreSnapshot(AddEntities(animate));
			}
			else
				result = false;

			if (snapshot.Player != null)
				result &= _player!.RestoreSnapshot(AddEntities(snapshot.Player));
			else
				result = false;

			return result;
		}

		public class Snapshot
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
	}
}
