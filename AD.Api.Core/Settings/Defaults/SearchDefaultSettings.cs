using AD.Api.Attributes.Services;
using AD.Api.Core.Ldap;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Settings
{
    public interface ISearchDefaults
    {
        string[] AllAttributes { get; }
        string[] Attributes { get; }
        DereferenceAlias DereferenceAlias { get; }
        bool IsGlobal { get; }
        SearchScope Scope { get; }
        int SizeLimit { get; }
        TimeSpan Timeout { get; }
    }

    [DynamicDependencyRegistration]
    public sealed class SearchDefaultSettings : ISearchDefaults
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string[] ISearchDefaults.Attributes => this.DefaultAttributes;
        public string[] DefaultAttributes { get; set; } = [];

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        string[] ISearchDefaults.AllAttributes => this.AllAttributes;
        internal string[] AllAttributes { get; set; } = [];
        public DereferenceAlias DereferenceAlias { get; set; } = DereferenceAlias.Never;
        internal bool IsGlobal { get; set; }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ISearchDefaults.IsGlobal => this.IsGlobal;
        public SearchScope Scope { get; set; } = SearchScope.Subtree;
        [Range(0, int.MaxValue, ErrorMessage = "The search request Size Limit cannot be less than 0.")]
        public int SizeLimit { get; set; }
        [Range(typeof(TimeSpan), "0", "30", MaximumIsExclusive = true)]
        public TimeSpan Timeout { get; set; } = LdapRequest.DefaultTimeout;
        public bool UseGlobalAttributes { get; set; }

        [DynamicDependencyRegistrationMethod]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private static void AddToServices(IServiceCollection services, IConfiguration configuration)
        {
            IConfigurationSection defaultsSection = configuration
                .GetSection("Settings")
                .GetSection("SearchDefaults");

            if (!defaultsSection.Exists())
            {
                return;
            }

            SearchDefaultSettings? settings = defaultsSection.Get<SearchDefaultSettings>(x => x.ErrorOnUnknownConfiguration = false);

            if (settings is null)
            {
                return;
            }

            services.AddSingleton<ISearchDefaults>(settings);
        }
    }
}

