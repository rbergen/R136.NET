using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace R136.Shell.Tools
{
	public class LanguageProvider : ILanguageProvider
	{
		private string? language = null;

		public IServiceProvider? Services { private get; set; }

		public string Language
		{
			get
			{
				if (this.language == null && Services != null)
					this.language = Services.GetService<Status>()?.Language ?? Services.GetRequiredService<IConfiguration>()[Constants.DefaultLanguage];

				return this.language ?? Constants.Dutch;
			}

			set
			{
				if (this.language != value && Services != null)
				{
					if (!Services.GetRequiredService<IConfiguration>().GetSection(Constants.Languages).GetSection(value).Exists())
						return;

					var status = Services.GetService<Status>();
					if (status != null)
						status.Language = value;
				}

				this.language = value;
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
