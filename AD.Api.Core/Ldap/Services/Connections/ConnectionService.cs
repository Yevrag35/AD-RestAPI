using AD.Api.Attributes.Services;
using AD.Api.Core.Security;
using AD.Api.Core.Security.Encryption;
using AD.Api.Core.Settings;
using AD.Api.Exceptions;
using AD.Api.Startup.Exceptions;
using System.Collections.Frozen;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Net;
using System.Runtime.Versioning;

namespace AD.Api.Core.Ldap.Services.Connections
{
    public interface IConnectionService
    {
        bool TryGetConnection(string? key, [NotNullWhen(true)] out LdapConnection? connection);
    }

    [DynamicDependencyRegistration]
    internal sealed class ConnectionService : IConnectionService
    {
        private const string DEFAULT = "Default";
        internal FrozenDictionary<string, ConnectionContext> Contexts { get; }

        private ConnectionService(FrozenDictionary<string, ConnectionContext> pairs)
        {
            this.Contexts = pairs;
        }

        public bool TryGetConnection(string? key, [NotNullWhen(true)] out LdapConnection? connection)
        {
            key ??= DEFAULT;

            if (!this.Contexts.TryGetValue(key, out ConnectionContext? context))
            {
                connection = null;
                return false;
            }

            connection = context.CreateConnection();
            return true;
        }

        [DynamicDependencyRegistrationMethod]
        [EditorBrowsable(EditorBrowsableState.Never)]
        private static void AddToServices(IServiceCollection services)
        {
            services.AddSingleton<IConnectionService>(provider =>
            {
                IConfiguration configuration = provider.GetRequiredService<IConfiguration>();
                IConfigurationSection domains = configuration.GetSection("Domains");
                IEncryptionService encSvc = provider.GetRequiredService<IEncryptionService>();
                var dict = ReadCredentialsFromConfig(domains, encSvc).ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
                return new ConnectionService(dict);
            });
        }
        private static ConnectionContext CreateContextFromResult(string key, RegisteredDomain domain, IEncryptionResult result)
        {
            if (result.HasCredential && result.Credential.IsEmpty)
            {
                if (!OperatingSystem.IsWindows())
                {
                    throw new NotSupportedException("Negotiate is only supported on Windows platforms.");
                }

                return new NegotiateContext(domain, key);
            }
            else if (result.HasCredential && OperatingSystem.IsWindows())
            {
                return new ChallengeContext(domain, key, result.Credential);
            }
            else
            {
                throw new NotSupportedException("Coming soon.");
            }
        }
        private static Dictionary<string, ConnectionContext> ReadCredentialsFromConfig(IConfigurationSection domainsSection, IEncryptionService encryptionService)
        {
            Dictionary<string, ConnectionContext> dict = new(1, StringComparer.OrdinalIgnoreCase);
            List<ValidationResult> results = [];
            if (domainsSection.Exists())
            {
                foreach (IConfigurationSection domain in domainsSection.GetChildren())
                {
                    RegisteredDomain info = ReadDomainFromConfig(domain);
                    info.Name = domain.Key;
                    if (string.IsNullOrWhiteSpace(info.DomainName))
                    {
                        info.DomainName = domain.Key;
                    }

                    var result = encryptionService.ReadCredentials(domain);
                    results.AddRange(result.Errors);

                    if (results.Count == 0)
                    {
                        _ = dict.TryAdd(domain.Key, CreateContextFromResult(domain.Key, info, result));
                    }
                }
            }

            if (dict.Count <= 0)
            {
                if (!OperatingSystem.IsWindows())
                {
                    throw new AdApiStartupException(typeof(ConnectionService), "No domains were found in the configuration.");   
                }

                using Forest forest = GetForest();
                NegotiateContext context = new(forest, "Default");
                _ = dict.TryAdd(context.Name, context);
            }

            StartupValidationException.ThrowIfNotEmpty<ConnectionService>(results);
            return dict;
        }
        private static RegisteredDomain ReadDomainFromConfig(IConfigurationSection domain)
        {
            RegisteredDomain? parsed = domain.Get<RegisteredDomain>(x => x.ErrorOnUnknownConfiguration = false);
            if (parsed is null)
            {
                throw new AdApiStartupException(typeof(ConnectionService), $"Unable to parse the settings for the domain '{domain.Key}'.");
            }

            return parsed;
        }
        [SupportedOSPlatform("WINDOWS")]
        private static Forest GetForest()
        {
            try
            {
                return Forest.GetCurrentForest();
            }
            catch (ActiveDirectoryOperationException e)
            {
                throw new AdApiStartupException(typeof(ConnectionService), e);
            }
        }
    }
}

