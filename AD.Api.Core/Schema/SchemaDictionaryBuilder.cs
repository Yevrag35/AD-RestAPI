using AD.Api.Core.Ldap;
using System.Collections.Concurrent;
using System.DirectoryServices.ActiveDirectory;
using System.Runtime.Versioning;

namespace AD.Api.Core.Schema
{
    [SupportedOSPlatform("WINDOWS")]
    internal sealed class SchemaDictionaryBuilder : IDisposable
    {
        private bool _disposed;
        private RegisteredDomain _domain = null!;
        private ActiveDirectorySchema _schema = null!;
        private readonly ConcurrentDictionary<string, SchemaProperty> _dict;
        private SemaphoreSlim _semaphore;
        private SemaphoreSlim _clsSema;

        public int Count => _dict.Count;

        internal SchemaDictionaryBuilder(ConnectionContext context)
        {
            _dict = new(Environment.ProcessorCount, 1000, StringComparer.OrdinalIgnoreCase);
            _semaphore = new(20);
            _clsSema = new(10, 10);
            context.SetSchemaBuilder(this, (dom, ctx, b) =>
            {
                b._schema = ActiveDirectorySchema.GetSchema(ctx);
                b._domain = dom;
            });
        }

        public SchemaClassPropertyDictionary Build()
        {
            return new(_domain.Name, _domain.DomainName, _dict);
        }
        public async Task ReadFromAsync(string className, CancellationToken token = default)
        {
            if (!this.TryGetClass(className, out var schemaClass))
            {
                return;
            }

            try
            {
                using (schemaClass)
                {
                    var allProps = schemaClass.GetAllProperties();
                    List<Task> tasks = new(allProps.Count);
                    await _clsSema.WaitAsync(token).ConfigureAwait(false);

                    foreach (ActiveDirectorySchemaProperty schProp in allProps)
                    {
                        tasks.Add(this.ReadInPropertyAsync(schProp, token));
                    }

                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
            }
            finally
            {
                _clsSema.Release();
            }
        }

        private async Task ReadInPropertyAsync(ActiveDirectorySchemaProperty schemaProperty, CancellationToken token)
        {
            try
            {
                await _semaphore.WaitAsync(token).ConfigureAwait(false);
                await Task.Factory.StartNew(p =>
                {
                    var sp = ((ConcurrentDictionary<string, SchemaProperty>, ActiveDirectorySchemaProperty))p!;
                    Property prop = Property.Create(sp.Item2);

                    if (prop.IsDefunct)
                    {
                        return;
                    }

                    if (prop.LinkId.HasValue && prop.Name.EndsWith("BL", StringComparison.Ordinal))
                    {
                        return;
                    }

                    if (!sp.Item1.ContainsKey(prop.Name))
                    {
                        var schema = SchemaProperty.Create(prop.Name, prop.Syntax, prop.IsSingleValued);
                        sp.Item1.TryAdd(prop.Name, schema);
                    }
                }, (_dict, schemaProperty), token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) { }
            finally
            {
                try
                {
                    _semaphore.Release();
                }
                catch (ObjectDisposedException) { }
            }
        }

        private bool TryGetClass(string className, [NotNullWhen(true)] out ActiveDirectorySchemaClass? schemaClass)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            try
            {
                schemaClass = _schema.FindClass(className);
                return true;
            }
            catch (ActiveDirectoryObjectNotFoundException e)
            {
                Debug.Fail(e.Message);
                schemaClass = null;
                return false;
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
                _dict.Clear();
                if (disposing)
                {
                    _semaphore.Dispose();
                    _schema?.Dispose();
                    _clsSema?.Dispose();
                }

                _schema = null!;
                _clsSema = null!;
                _semaphore = null!;
                _disposed = true;
            }
        }

        private readonly struct Property
        {
            public readonly string Name;
            public readonly int? LinkId;
            public readonly bool IsDefunct;
            public readonly bool IsSingleValued;
            public readonly ActiveDirectorySyntax Syntax;

            private Property(string name, int? linkId, bool isDefunct, ActiveDirectorySyntax syntax, bool isSingle)
            {
                Name = name;
                LinkId = linkId;
                IsDefunct = isDefunct;
                IsSingleValued = isSingle;
                Syntax = syntax;
            }

            private static Property Empty => new(string.Empty, null, true, ActiveDirectorySyntax.Bool, true);
            internal static Property Create(ActiveDirectorySchemaProperty? property)
            {
                if (property is null)
                {
                    return Empty;
                }

                string name;
                int? linkId = null;
                bool isDefunct = true;
                bool isSingle = true;
                ActiveDirectorySyntax syntax;
                try
                {
                    name = property.Name;
                    linkId = property.LinkId;
                    isDefunct = property.IsDefunct;
                    syntax = property.Syntax;
                    isSingle = property.IsSingleValued;
                }
                catch (Exception e)
                {
                    Debug.Fail(e.Message);
                    return Empty;
                }

                return new(name, linkId, isDefunct, syntax, isSingle);
            }

            public static explicit operator Property(ActiveDirectorySchemaProperty? property)
            {
                return Create(property);
            }
        }
    }
}

