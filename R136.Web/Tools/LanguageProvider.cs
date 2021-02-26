using Blazored.LocalStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;


#nullable enable

namespace R136.Web.Tools
{
	public class LanguageProvider : ILanguageProvider
	{
		private string? _language = null;

		public IServiceProvider? Services { private get; set; }

		public string Language
		{
			get {
				if (_language == null)
				{
					if (Services != null)
					{
						var localStorage = Services.GetService<ILocalStorageService>();
						if (localStorage != null)
						{
							if (localStorage.ContainKeyAsync(Constants.LanguageStorageKey).AsTask().Result)
								_language = localStorage.GetItemAsync<string>(Constants.LanguageStorageKey).AsTask().Result;
						}
						else
							_language = Services.GetRequiredService<IConfiguration>()[Constants.DefaultLanguage];
					}
				}

				return _language!;
			} 

			set
			{
				if (_language != value && Services != null)
				{
					if (!Services.GetRequiredService<IConfiguration>().GetSection(Constants.Languages).GetSection(value).Exists())
						return;

					var localStorage = Services.GetService<ILocalStorageService>();
					if (localStorage != null)
						localStorage.SetItemAsync(Constants.LanguageStorageKey, value).AsTask().Wait();
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
		string? GetConfigurationValue(string key);
	}
}

#nullable restore