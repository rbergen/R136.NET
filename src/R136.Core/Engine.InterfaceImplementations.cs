using Microsoft.Extensions.DependencyInjection;
using R136.Entities;
using R136.Entities.Global;
using R136.Interfaces;
using System;

namespace R136.Core
{
	public partial class Engine : IStatusManager, IGameServiceProvider, IGameServiceBasedConfigurator, ITurnEndingProvider, ISnappable<Engine.Snapshot>
	{
		public void Place(ItemID item)
		{
			if (!IsInitialized)
				throw new InvalidOperationException(EngineNotInitialized);

			PlaceAt(item, _player!.CurrentRoom);
		}

		public void OpenConnection(Direction direction, RoomID toRoom)
		{
			if (!IsInitialized)
				throw new InvalidOperationException(EngineNotInitialized);

			if (!_player!.CurrentRoom.Connections.ContainsKey(direction))
				_player!.CurrentRoom.Connections[direction] = _rooms![toRoom];
		}

		public void RegisterServices(IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<IStatusManager>(this);
			serviceCollection.AddSingleton<ITurnEndingProvider>(this);
		}

		public void Configure(IServiceProvider serviceProvider)
		{
			var fireProvider = serviceProvider.GetService<IFireNotificationProvider>();

			if (fireProvider != null)
				fireProvider.Burned += HandleBurning;			
		}

		public void RegisterListener(INotifyTurnEnding listener)
		=> _turnEndingNotifiees.Add(listener.TurnEnding);

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
			Item.TakeSnapshots(snapshot, _items!);
			Animate.TakeSnapshots(snapshot, _animates!);
			snapshot.Player = _player!.TakeSnapshot();

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

			Item.RestoreSnapshots(snapshot, _items!);
			Animate.RestoreSnapshots(snapshot, _animates!);

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
	}
}
