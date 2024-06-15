using AD.Api.Core.Ldap.Services.Connections;
using AD.Api.Core.Settings;
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
        private readonly Dictionary<string, SchemaProperty> _dict;

        public int Count => _dict.Count;

        internal SchemaDictionaryBuilder(ConnectionContext context)
        {
            _dict = new(5000, StringComparer.OrdinalIgnoreCase);
            context.SetSchemaBuilder(this, (dom, ctx, b) =>
            {
                b._schema = ActiveDirectorySchema.GetSchema(ctx);
                b._domain = dom;
            });
        }

        public SchemaClassPropertyDictionary Build()
        {
            return new(_domain.DomainName, _dict);
        }
        public void ReadFrom(string className)
        {
            if (!this.TryGetClass(className, out var schemaClass))
            {
                return;
            }

            using (schemaClass)
            {
                int cap = _dict.EnsureCapacity(schemaClass.MandatoryProperties.Count + schemaClass.OptionalProperties.Count);
                Debug.WriteLine($"Capacity: {cap}");

                foreach (Property prop in schemaClass.GetAllProperties())
                {
                    if (prop.IsDefunct)
                    {
                        continue;
                    }

                    if (prop.LinkId.HasValue && prop.Name.EndsWith("BL", StringComparison.Ordinal))
                    {
                        continue;
                    }
                    
                    if (!_dict.ContainsKey(prop.Name))
                    {
                        _dict.Add(prop.Name, SchemaProperty.Create(prop.Name, prop.Syntax, prop.IsSingleValued));
                    }
                }
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
                    _schema?.Dispose();
                }

                _schema = null!;
                _disposed = true;
            }
        }

        private readonly ref struct Property
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
            private static Property Create(ActiveDirectorySchemaProperty? property)
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

