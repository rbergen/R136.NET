using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace R136.Entities.General
{
	public class Logger
	{
		public IServiceProvider? Services { private get; set; }

		private readonly Dictionary<Type, ILogger> loggerMap = new();

		public void Log<TCaller>(LogLevel level, string message)
		{
			if (!this.loggerMap.TryGetValue(typeof(TCaller), out var logger))
			{
				if (Services != null)
					logger = Services.GetService<ILogger<TCaller>>();

				if (logger == null)
					return;

				this.loggerMap[typeof(TCaller)] = logger;
			}

			logger.Log(level, "{Message}", message);
		}

		public void LogDebug<TCaller>(string message)
			=> Log<TCaller>(LogLevel.Debug, message);

		public void LogError<TCaller>(string message)
			=> Log<TCaller>(LogLevel.Error, message);

	}
}
