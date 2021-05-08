using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;


#nullable enable

namespace R136.Web.Tools
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
				{
					var localStorage = Services.GetService<ISyncLocalStorageService>();
					if (localStorage != null && localStorage.ContainKey(Constants.LanguageStorageKey))
						this.language = localStorage.GetItem<string>(Constants.LanguageStorageKey);

					if (this.language == null)
						this.language = Services.GetRequiredService<IConfiguration>()[Constants.DefaultLanguage];
				}

				return this.language ?? Constants.Dutch;
			}

			set
			{
				if (this.language != value && Services != null)
				{
					if (!Services.GetRequiredService<IConfiguration>().GetSection(Constants.Languages).GetSection(value).Exists())
						return;

					var localStorage = Services.GetService<ISyncLocalStorageService>();
					if (localStorage != null)
						localStorage.SetItem(Constants.LanguageStorageKey, value);
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
		string? GetConfigurationValue(string key);
	}
}

#nullable restore