using Microsoft.Extensions.DependencyInjection;
using R136.Interfaces;
using System;

namespace R136.Entities
{
	public abstract class EntityBase
	{
		private static IServiceProvider? gameServices;
		
		private static IPlayer? player = null;
		private static readonly object playerLock = new();
		
		private static IStatusManager? statusManager = null;
		private static readonly object statusManagerLock = new();

		public static IServiceProvider? GameServices 
		{
			set 
			{
				gameServices = value;
				lock (playerLock)
				{
					player = null;
				}
				lock (statusManagerLock)
				{
					statusManager = null;
				}
			} 
		}

		protected static IPlayer? Player
		{
			get
			{
				lock (playerLock)
				{
					if (player == null && gameServices != null)
						player = gameServices.GetService<IPlayer>();

					return player;
				}
			}
		}

		protected static IStatusManager? StatusManager
		{
			get
			{
				lock (statusManagerLock)
				{
					if (statusManager == null && gameServices != null)
						statusManager = gameServices.GetService<IStatusManager>();

					return statusManager;
				}
			}
		}
	}
}
