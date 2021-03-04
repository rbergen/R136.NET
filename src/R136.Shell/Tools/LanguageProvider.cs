using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace R136.Shell.Tools
{
	public class LanguageProvider : ILanguageProvider
	{
		private string? _language = null;

		public IServiceProvider? Services { private get; set; }

		public string Language
		{
			get
			{
				if (_language == null && Services != null)
					_language = Services.GetService<Status>()?.Language ?? Services.GetRequiredService<IConfiguration>()[Constants.DefaultLanguage];

				return _language ?? Constants.Dutch;
			}

			set
			{
				if (_language != value && Services != null)
				{
					if (!Services.GetRequiredService<IConfiguration>().GetSection(Constants.Languages).GetSection(value).Exists())
						return;

					var status = Services.GetService<Status>();
					if (status != null)
						status.Language = value;
				}

				_language = value;
			}
		}

		public string? GetConfigurationValue(string key)
			=> Services?.GetRequiredService<IConfiguration>().GetSection(Constants.Languages).GetSection(Language)[key];
	}

	public interface ILanguageProvider
	{
		IServiceProvider? Services { set; }
		string Language { get; set; }
		public string? GetConfigurationValue(string key);

	}
}
