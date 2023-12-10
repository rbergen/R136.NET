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

			PlaceAt(item, this.player!.CurrentRoom);
		}

		public void OpenConnection(Direction direction, RoomID toRoom)
		{
			if (!IsInitialized)
				throw new InvalidOperationException(EngineNotInitialized);

			if (!this.player!.CurrentRoom.Connections.ContainsKey(direction))
				this.player!.CurrentRoom.Connections[direction] = this.rooms![toRoom];
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
		=> this.turnEndingNotifiees.Add(listener.TurnEnding);

		public Snapshot TakeSnapshot(Snapshot? snapshot = null)
		{
			if (!IsInitialized)
				throw new InvalidOperationException(EngineNotInitialized);

			snapshot ??= new Snapshot();
			snapshot.Configuration = Facilities.Configuration;
			snapshot.HasTreeBurned = this.hasTreeBurned;
			snapshot.DoNext = DoNext;
			snapshot.LocationCommandProcessor = this.processors!.LocationProcessor.TakeSnapshot();
			Item.TakeSnapshots(snapshot, this.items!);
			Animate.TakeSnapshots(snapshot, this.animates!);
			snapshot.Player = this.player!.TakeSnapshot();

			return snapshot;
		}

		public bool RestoreSnapshot(Snapshot snapshot)
		{
			if (!IsInitialized)
				throw new InvalidOperationException(EngineNotInitialized);

			bool result = true;

			if (snapshot.Configuration != null)
				Facilities.Configuration.Load(snapshot.Configuration);

			this.hasTreeBurned = snapshot.HasTreeBurned;
			DoNext = snapshot.DoNext;

			if (snapshot.LocationCommandProcessor != null)
				result &= this.processors!.LocationProcessor.RestoreSnapshot(AddEntities(snapshot.LocationCommandProcessor));
			else
				result = false;

			Item.RestoreSnapshots(snapshot, this.items!);
			Animate.RestoreSnapshots(snapshot, this.animates!);

			if (snapshot.Animates != null)
			{
				foreach (var animate in snapshot.Animates)
					result &= this.animates![animate.ID].RestoreSnapshot(AddEntities(animate));
			}
			else
				result = false;

			if (snapshot.Player != null)
				result &= this.player!.RestoreSnapshot(AddEntities(snapshot.Player));
			else
				result = false;

			return result;
		}
	}
}
