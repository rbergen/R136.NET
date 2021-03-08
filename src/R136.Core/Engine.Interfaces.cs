using Microsoft.Extensions.DependencyInjection;
using R136.Interfaces;
using System;

namespace R136.Core
{
	public partial class Engine : IStatusManager, IGameServiceProvider, IGameServiceBasedConfigurator, ITurnEndingProvider
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

		public void Configure(IServiceProvider serviceProvider)
		{
			var fireProvider = serviceProvider.GetService<IFireNotificationProvider>();

			if (fireProvider != null)
				fireProvider.Burned += HandleBurning;			
		}

		public void RegisterListener(INotifyTurnEnding listener)
		=> _turnEndingNotifiees.Add(listener.TurnEnding);

		public void RegisterServices(IServiceCollection serviceCollection)
		{
			serviceCollection.AddSingleton<IStatusManager>(this);
			serviceCollection.AddSingleton<ITurnEndingProvider>(this);
		}

	}
}
