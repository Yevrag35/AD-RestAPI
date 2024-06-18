using AD.Api.Attributes.Services;
using AD.Api.Core.Ldap.Services.Connections;
using AD.Api.Core.Ldap.Services.Schemas;
using AD.Api.Core.Services;
using ConcurrentCollections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.Versioning;
using ConDict = System.Collections.Concurrent.ConcurrentDictionary<AD.Api.Core.Ldap.Services.Connections.ConnectionContext, AD.Api.Core.Schema.SchemaClassPropertyDictionary>;
using SchDict = System.Collections.Concurrent.ConcurrentDictionary<string, AD.Api.Core.Schema.SchemaClassPropertyDictionary>;

namespace AD.Api.Core.Schema
{
    [DynamicDependencyRegistration]
    public sealed class SchemaLoadingService : StartupServiceBase, IDisposable
    {
        private bool _disposed;
        private SemaphoreSlim _semaphore;

        public SchemaLoadingService(IServiceProvider provider)
            : base(provider)
        {
            _semaphore = new(3, 3);
        }

        protected override async Task StartingAsync(IServiceProvider provider, CancellationToken cancellationToken)
        {
            SchemaService schemaSvc = provider.GetRequiredService<SchemaService>();
            if (!OperatingSystem.IsWindows() || !schemaSvc.IsFunctional)
            {
                return;
            }

            SchDict fullDict = new(Environment.ProcessorCount, 2, StringComparer.OrdinalIgnoreCase);
            IConnectionService connectionSvc = provider.GetRequiredService<IConnectionService>();

             await LoadAllSchemasAsync(connectionSvc, fullDict, schemaSvc, cancellationToken).ConfigureAwait(false);

            schemaSvc.AddSchemaDictionary(fullDict);
        }

        [SupportedOSPlatform("WINDOWS")]
        private static async Task LoadAllSchemasAsync(IConnectionService connectionSvc, SchDict fullDict, SchemaService schemaSvc, CancellationToken token)
        {
            Dictionary<ConnectionContext, ConcurrentHashSet<string>> constructed = new(4);
            List<Task<SchemaClassPropertyDictionary>> tasks = new(connectionSvc.RegisteredConnections.Count);
            using SemaphoreSlim semaphore = new(1, 1);

            foreach (string key in connectionSvc.RegisteredConnections.Keys)
            {
                ConnectionContext context = connectionSvc.RegisteredConnections[key];
                if (!context.IsForestRoot)
                {
                    continue;
                }

                if (!constructed.TryGetValue(context, out ConcurrentHashSet<string>? nameSet))
                {
                    tasks.Add(SchemaLoader.LoadSchemaAsync(context, semaphore, schemaSvc.ClassNames, token));
                    nameSet = new(Environment.ProcessorCount, 4, StringComparer.OrdinalIgnoreCase);
                    constructed.TryAdd(context, nameSet);
                }

                nameSet.Add(key);
            }

            while (tasks.Count > 0)
            {
                var completed = await Task.WhenAny(tasks).ConfigureAwait(false);
                tasks.Remove(completed);

                SchemaClassPropertyDictionary dict = await completed.ConfigureAwait(false);

                fullDict.TryAdd(dict.DomainName, dict);
                if (dict.DomainKey != dict.DomainName)
                {
                    fullDict.TryAdd(dict.DomainKey, dict);
                }

                ConnectionContext ctx = connectionSvc.RegisteredConnections[dict.DomainKey];
                if (ctx.IsDefault)
                {
                    if (dict.DomainKey == string.Empty)
                    {
                        fullDict.TryAdd("Default", dict);
                    }
                    else if (dict.DomainKey == "Default")
                    {
                        fullDict.TryAdd(string.Empty, dict);
                    }
                    else if (dict.DomainKey == ctx.DomainName)
                    {
                        fullDict.TryAdd("Default", dict);
                        fullDict.TryAdd(string.Empty, dict);
                    }
                }
            }
        }

        public void Dispose()
        {
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _semaphore?.Dispose();
                }

                _semaphore = null!;
                _disposed = true;
            }
        }

        [DynamicDependencyRegistrationMethod]
        private static void AddToServices(IServiceCollection services)
        {
            services.AddHostedService<SchemaLoadingService>();
        }
    }
}

