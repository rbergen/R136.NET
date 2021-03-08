using Microsoft.Extensions.DependencyInjection;
using R136.Entities.Global;
using R136.Interfaces;
using System;

namespace R136.Entities
{
	public abstract class EntityBase
	{
		private static IServiceProvider? _gameServices;
		
		private static IPlayer? _player = null;
		private static readonly object _playerLock = new();
		
		private static IStatusManager? _statusManager = null;
		private static readonly object _statusManagerLock = new();

		public static IServiceProvider? GameServices 
		{
			set 
			{
				_gameServices = value;
				lock (_playerLock)
				{
					_player = null;
				}
				lock (_statusManagerLock)
				{
					_statusManager = null;
				}
			} 
		}

		protected static IPlayer? Player
		{
			get
			{
				lock (_playerLock)
				{
					if (_player == null && _gameServices != null)
						_player = _gameServices.GetService<IPlayer>();

					return _player;
				}
			}
		}

		protected static IStatusManager? StatusManager
		{
			get
			{
				lock (_statusManagerLock)
				{
					if (_statusManager == null && _gameServices != null)
						_statusManager = _gameServices.GetService<IStatusManager>();

					return _statusManager;
				}
			}
		}
	}
}
