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
        int TotalDefaultAttributeCount { get; }
        int TotalGlobalAttributeCount { get; }

        ref readonly ISearchDefaults this[string key] { get; }

        int GetAttributeCount(FilteredRequestType types);
        int GetAttributeCount(FilteredRequestType types, bool includeGlobal);

        bool TryGetAllAttributes(FilteredRequestType types, Span<string> attributes, out int count);
        bool TryGetAllAttributes(FilteredRequestType types, Span<string> attributes, bool includeGlobal, out int count);

        bool TryGetFirstDefaults(FilteredRequestType types, [NotNullWhen(true)] out ISearchDefaults? defaults);
    }

    [DynamicDependencyRegistration]
    internal sealed class DefaultsService : IDefaults
    {
        private readonly FrozenDictionary<string, ISearchDefaults> _dictionary;

        public ref readonly ISearchDefaults this[string key] => ref _dictionary[key];

        public IEnumStrings<FilteredRequestType> RequestTypes { get; }
        public int TotalDefaultAttributeCount { get; }
        public int TotalGlobalAttributeCount { get; }

        private DefaultsService(IEnumStrings<FilteredRequestType> types, IDictionary<string, ISearchDefaults> dictionary)
        {
            _dictionary = dictionary.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
            this.TotalGlobalAttributeCount = _dictionary[string.Empty].Attributes.Length;
            this.TotalDefaultAttributeCount = _dictionary.Values
                .Where(x => !x.IsGlobal)
                .Sum(x => x.Attributes.Length);

            this.RequestTypes = types;
        }

        [DebuggerStepThrough]
        public int GetAttributeCount(FilteredRequestType types)
        {
            return this.GetAttributeCount(types, includeGlobal: true);
        }
        public int GetAttributeCount(FilteredRequestType types, bool includeGlobal)
        {
            int count = includeGlobal ? this.TotalGlobalAttributeCount : 0;
            FlagEnumerator<FilteredRequestType> enumerator = new(types);

            while (enumerator.MoveNext())
            {
                if (this.RequestTypes.TryGetName(enumerator.Current, out string? name)
                    &&
                    _dictionary.TryGetValue(name, out var defaults))
                {
                    count += defaults.Attributes.Length;
                }
            }

            return count;
        }

        [DebuggerStepThrough]
        public bool TryGetAllAttributes(FilteredRequestType types, Span<string> attributes, out int count)
        {
            return this.TryGetAllAttributes(types, attributes, includeGlobal: true, out count);
        }
        public bool TryGetAllAttributes(FilteredRequestType types, Span<string> attributes, bool includeGlobal, out int count)
        {
            FlagEnumerator<FilteredRequestType> enumerator = new(types);
            count = includeGlobal ? this.TotalGlobalAttributeCount : 0;
            int nonDefaultCount = 0;

            while (enumerator.MoveNext())
            {
                if (this.TryGetDefaultsFromFlag(enumerator.Current, out ISearchDefaults? defaults))
                {
                    defaults.Attributes.CopyTo(attributes.Slice(nonDefaultCount));
                    nonDefaultCount += defaults.Attributes.Length;
                }
            }

            if (nonDefaultCount > 0)
            {
                if (includeGlobal)
                {
                    _dictionary[string.Empty].Attributes.CopyTo(attributes.Slice(nonDefaultCount));
                }

                count += nonDefaultCount;

                return true;
            }
            else
            {
                return false;
            }
        }
        public bool TryGetFirstDefaults(FilteredRequestType types, [NotNullWhen(true)] out ISearchDefaults? defaults)
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

        private bool TryGetDefaultsFromFlag(FilteredRequestType singleType, [NotNullWhen(true)] out ISearchDefaults? defaults)
        {
            defaults = default;

            return this.RequestTypes.TryGetName(singleType, out string? name)
                   && 
                   _dictionary.TryGetValue(name, out defaults);
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

