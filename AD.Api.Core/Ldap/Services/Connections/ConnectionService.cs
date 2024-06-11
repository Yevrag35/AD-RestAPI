using AD.Api.Attributes.Services;
using AD.Api.Core.Security;
using AD.Api.Core.Security.Encryption;
using AD.Api.Core.Settings;
using AD.Api.Exceptions;
using AD.Api.Startup.Exceptions;
using OneOf;
using System.Collections.Frozen;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.DirectoryServices.ActiveDirectory;
using System.DirectoryServices.Protocols;
using System.Linq.Expressions;
using System.Net;
using System.Runtime.Versioning;

namespace AD.Api.Core.Ldap.Services.Connections
{
    public interface IConnectionService
    {
        ContextLibrary RegisteredConnections { get; }
        OneOf<LdapConnection, T> GetConnectionOr<T>(string? key, Expression<Func<string, T>> errorExpression);
    }

    [DynamicDependencyRegistration]
    internal sealed class ConnectionService : IConnectionService
    {
        private const string DEFAULT = "Default";
        public ContextLibrary RegisteredConnections { get; }

        private ConnectionService(Dictionary<string, ConnectionContext> pairs)
        {
            this.RegisteredConnections = new(pairs);
        }

        public OneOf<LdapConnection, T> GetConnectionOr<T>(string? key, Expression<Func<string, T>> errorExpression)
        {
            if (!this.TryGetConnection(key, out LdapConnection? connection))
            {
                T error = errorExpression.Compile().Invoke(key);
                return error;
            }

            return connection;
        }
        public bool TryGetConnection([NotNullWhen(false)] string? key, [NotNullWhen(true)] out LdapConnection? connection)
        {
            if (!this.RegisteredConnections.TryGetValue(key, out ConnectionContext? context))
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
                var dict = ReadCredentialsFromConfig(domains, encSvc);
                return new ConnectionService(dict);
            });
        }
        private static void AddDefaultContext(ConnectionContext? defaultContext, Dictionary<string, ConnectionContext> contexts)
        {
            if (defaultContext is not null)
            {
                contexts[DEFAULT] = defaultContext;
                contexts[string.Empty] = defaultContext;
            }
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
        private static Dictionary<string, ConnectionContext> ReadCredentialsFromConfig(IConfigurationSection domainsSection, IEncryptionService encryptionService)
        {
            Dictionary<string, ConnectionContext> dict = new(1, StringComparer.OrdinalIgnoreCase);
            ConnectionContext? defaultContext = null;

            List<ValidationResult> results = [];
            if (domainsSection.Exists())
            {
                foreach (IConfigurationSection domain in domainsSection.GetChildren())
                {
                    if (string.IsNullOrWhiteSpace(domain.Key))
                    {
                        results.Add(new ValidationResult("A registered domain must have a name (key) identifier for connections.", [nameof(domain)]));

                        continue;
                    }

                    RegisteredDomain info = ReadDomainFromConfig(domain);

                    info.Name = domain.Key;
                    if (string.IsNullOrWhiteSpace(info.DomainName))
                    {
                        info.DomainName = domain.Key;
                    }

                    var result = encryptionService.ReadCredentials(domain);
                    results.AddRange(result.Errors);

                    if (TryCreateContextFromResult(domain.Key, info, result, results, out var context))
                    {
                        SetDefaultContext(context, info, ref defaultContext);
                        _ = dict.TryAdd(domain.Key, context);
                        _ = dict.TryAdd(info.DomainName, context);
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
                defaultContext = new NegotiateContext(forest, DEFAULT);
                _ = dict.TryAdd(forest.Name, defaultContext);
                _ = dict.TryAdd(forest.RootDomain.Name, defaultContext);
                using var de = forest.RootDomain.GetDirectoryEntry();
                _ = dict.TryAdd((string)de.Properties["name"].Value!, defaultContext);
            }

            AddDefaultContext(defaultContext, dict);

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
        private static void SetDefaultContext(ConnectionContext context, RegisteredDomain domain, ref ConnectionContext? defaultContext)
        {
            if (domain.IsDefault && defaultContext is null)
            {
                defaultContext = context;
            }
        }
        private static bool TryCreateContextFromResult(string key, RegisteredDomain domain, IEncryptionResult result, List<ValidationResult> errors, [NotNullWhen(true)] out ConnectionContext? context)
        {
            context = null;
            if (result.Errors.Count > 0)
            {
                return false;
            }

            if (result.HasCredential && result.Credential.IsEmpty)
            {
                if (!OperatingSystem.IsWindows())
                {
                    errors.Add(new ValidationResult($"'{key}' has no credentials and Negotitate is only supported on Windows platforms.", [key]));
                    context = null;
                    return false;
                }

                context = new NegotiateContext(domain, key);
                return true;
            }

            if (result.HasCredential && OperatingSystem.IsWindows())
            {
                context = new ChallengeContext(domain, key, result.Credential);
                return true;
            }

            errors.Add(new ValidationResult($"'{key}' - couldn't find a supportable authentication mechanism for this domain. Coming soon.", [key]));
            return false;
        }
    }
}

