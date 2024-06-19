using AD.Api.Attributes.Services;
using AD.Api.Components;
using AD.Api.Core.Ldap;
using AD.Api.Core.Settings;
using AD.Api.Enums;
using AD.Api.Startup.Exceptions;
using System.Collections.Frozen;
using System.ComponentModel;

namespace AD.Api.Core
{
    public interface IDefaults
    {
        bool TryGetFirst(FilteredRequestType types, [NotNullWhen(true)] out ISearchDefaults? defaults);
    }

    [DynamicDependencyRegistration]
    internal sealed class DefaultsService : IDefaults
    {
        private readonly FrozenDictionary<string, ISearchDefaults> _dictionary;

        public IEnumStrings<FilteredRequestType> RequestTypes { get; }

        private DefaultsService(IEnumStrings<FilteredRequestType> types, IDictionary<string, ISearchDefaults> dictionary)
        {
            _dictionary = dictionary.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
            this.RequestTypes = types;
        }

        public bool TryGetFirst(FilteredRequestType types, [NotNullWhen(true)] out ISearchDefaults? defaults)
        {
            FlagEnumerator<FilteredRequestType> enumerator = new(types);
            while (enumerator.MoveNext())
            {
                if (this.RequestTypes.TryGetName(enumerator.Current, out string? name)
                    &&
                    _dictionary.TryGetValue(name, out defaults))
                {
                    return true;
                }
            }

            defaults = null;
            return false;
        }

        [DynamicDependencyRegistrationMethod]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private static void AddToServices(IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection section = configuration
                .GetRequiredSection("Settings")
                .GetRequiredSection("SearchDefaults");

            if (!section.Exists())
            {
                throw new AdApiStartupException(typeof(DefaultsService), "The 'Settings:SearchDefaults' section is missing from the configuration.");
            }

            
            IConfigurationSection globalSection = section.GetRequiredSection("Global");
            if (!globalSection.Exists())
            {
                throw new AdApiStartupException(typeof(DefaultsService), "The 'Settings:SearchDefaults:Global' section is missing from the configuration.");
            }

            SearchDefaultSettings globalSettings = ParseDefaultsFor(globalSection);
            globalSettings.IsGlobal = true;
            globalSettings.UseGlobalAttributes = true;

            Dictionary<string, ISearchDefaults> dictionary = new(6, StringComparer.OrdinalIgnoreCase)
            {
                { "Global", globalSettings },
                { "Default", globalSettings },
                { string.Empty, globalSettings },
            };

            HashSet<string> globals = new(globalSettings.DefaultAttributes, StringComparer.OrdinalIgnoreCase);
            HashSet<string> working = new(globals.Comparer);

            foreach (IConfigurationSection child in section.GetChildren().Where(x => x.Key != "Global"))
            {
                working.Clear();
                SearchDefaultSettings settings = ParseDefaultsFor(child);
                settings.IsGlobal = false;
                if (settings.UseGlobalAttributes)
                {
                    working.UnionWith(globals);
                }

                working.UnionWith(settings.DefaultAttributes);
                settings.AllAttributes = [.. working];

                dictionary.Add(child.Key, settings);
            }

            dictionary.TrimExcess();

            services.AddSingleton<IDefaults>(x =>
            {
                return new DefaultsService(x.GetRequiredService<IEnumStrings<FilteredRequestType>>(), dictionary);
            });
        }
        private static SearchDefaultSettings ParseDefaultsFor(IConfigurationSection section)
        {
            SearchDefaultSettings? settings = section.Get<SearchDefaultSettings>(x => x.ErrorOnUnknownConfiguration = false);

            return settings ?? new SearchDefaultSettings();
        }
    }
}

