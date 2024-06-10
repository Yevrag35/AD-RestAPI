using AD.Api.Attributes.Services;
using AD.Api.Core.Security;
using AD.Api.Core.Security.Encryption;
using AD.Api.Core.Settings;
using AD.Api.Exceptions;
using AD.Api.Startup.Exceptions;
using System.Collections.Frozen;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.Protocols;

namespace AD.Api.Core.Ldap.Services.Connections
{
    public interface IConnectionService
    {
        bool TryGetConnection(string key, [NotNullWhen(true)] out LdapConnection? connection);
    }

    [DynamicDependencyRegistration]
    internal sealed class ConnectionService : IConnectionService
    {
        internal FrozenDictionary<string, ConnectionContext> Contexts { get; }

        private ConnectionService(FrozenDictionary<string, ConnectionContext> pairs)
        {
            this.Contexts = pairs;
        }

        public bool TryGetConnection(string key, [NotNullWhen(true)] out LdapConnection? connection)
        {
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
                IConfigurationSection domains = configuration.GetRequiredSection("Domains");
                IEncryptionService encSvc = provider.GetRequiredService<IEncryptionService>();
                var dict = ReadCredentialsFromConfig(domains, encSvc).ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
                return new ConnectionService(dict);
            });
        }
        private static ConnectionContext CreateContextFromResult(string key, RegisteredDomain domain, EncryptedCredential credential)
        {
            if (credential.IsEmpty)
            {
                if (!OperatingSystem.IsWindows())
                {
                    throw new NotSupportedException("Negotiate is only supported on Windows platforms.");
                }

                return new NegotiateContext(domain, key);
            }
            else if (OperatingSystem.IsWindows())
            {
                return new ChallengeContext(domain, key, credential);
            }
            else
            {
                throw new NotSupportedException("Coming soon.");
            }
        }
        private static IEnumerable<KeyValuePair<string, ConnectionContext>> ReadCredentialsFromConfig(IConfigurationSection domainsSection, IEncryptionService encryptionService)
        {
            List<ValidationResult> results = [];
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
                    yield return new KeyValuePair<string, ConnectionContext>(domain.Key, CreateContextFromResult(domain.Key, info, result.Credential));
                }
            }

            StartupValidationException.ThrowIfNotEmpty<ConnectionService>(results);
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
    }
}

