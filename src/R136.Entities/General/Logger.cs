using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace R136.Entities.General
{
	public class Logger
	{
		public IServiceProvider? Services { private get; set; }

		private readonly Dictionary<Type, ILogger> _loggerMap = new();

		public void Log<TCaller>(LogLevel level, string message)
		{
			if (!_loggerMap.TryGetValue(typeof(TCaller), out var _logger))
			{
				if (Services != null)
					_logger = Services.GetService<ILogger<TCaller>>();

				if (_logger == null)
					return;

				_loggerMap[typeof(TCaller)] = _logger;
			}

			_logger.Log(level, message);
		}

		public void LogDebug<TCaller>(string message)
			=> Log<TCaller>(LogLevel.Debug, message);

		public void LogError<TCaller>(string message)
			=> Log<TCaller>(LogLevel.Error, message);

	}
}
