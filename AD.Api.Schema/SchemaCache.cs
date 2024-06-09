using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices.ActiveDirectory;

namespace AD.Api.Schema
{
    public class SchemaCache
    {
        private readonly Dictionary<string, SchemaProperty> _cache;

        public SchemaProperty? this[string key]
        {
            get => this.TryGetValue(key, out SchemaProperty? schemaProperty) ? schemaProperty : null;
        }

        public int Count => _cache.Count;

        public SchemaCache(int capacity = 1000)
        {
            _cache = new Dictionary<string, SchemaProperty>(capacity, StringComparer.CurrentCultureIgnoreCase);
        }
        public SchemaCache(IEnumerable<SchemaProperty> collection)
        {
            _cache = collection.ToDictionary(key => key.Name, StringComparer.CurrentCultureIgnoreCase);
        }

        public bool Add(ActiveDirectorySchemaProperty property)
        {
            bool added = false;

            if (!this.ContainsKey(property.Name))
            {
                _cache.Add(property.Name, new SchemaProperty
                {
                    Class = property.SchemaGuid,
                    Name = property.Name,
                    IsInGlobalCatalog = property.IsInGlobalCatalog,
                    IsSingleValued = property.IsSingleValued,
                    RangeLower = property.RangeLower,
                    RangeUpper = property.RangeUpper
                });

                added = true;
            }

            return added;
        }

        public bool Add(SchemaProperty schemaProperty)
        {
            return _cache.TryAdd(schemaProperty.Name, schemaProperty);
        }

        public bool ContainsKey(string key) => _cache.ContainsKey(key);

        public bool TryGetValue(string? key, [NotNullWhen(true)] out SchemaProperty? schemaProperty)
        {
            return _cache.TryGetValue(key ?? string.Empty, out schemaProperty);
        }
    }
}