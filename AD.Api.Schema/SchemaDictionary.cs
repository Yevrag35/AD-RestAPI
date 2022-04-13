using MG.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Schema
{
    public class SchemaDictionary : IEnumerable<SchemaProperty>
    {
        private readonly Dictionary<string, UniqueList<SchemaProperty>> _classToProp;
        private readonly Dictionary<string, SchemaProperty> _attributeToProp;

        public SchemaProperty? this[string attributeName] => _attributeToProp
            .TryGetValue(attributeName, out SchemaProperty? property)
            ? property
            : null;

        public ICollection<string> Classes => _classToProp.Keys;
        public int Count => _attributeToProp.Count;
        public ICollection<SchemaProperty> Values => _attributeToProp.Values;

        public SchemaDictionary()
        {
            _attributeToProp = new Dictionary<string, SchemaProperty>(5000, StringComparer.CurrentCultureIgnoreCase);
            _classToProp = new Dictionary<string, UniqueList<SchemaProperty>>(5000, StringComparer.CurrentCultureIgnoreCase);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="property"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public bool Add(ActiveDirectorySchemaProperty property, string className)
        {
            if (string.IsNullOrWhiteSpace(className))
                throw new ArgumentNullException(nameof(className));

            if (property is null)
                throw new ArgumentNullException(nameof(property));

            if (!_attributeToProp.ContainsKey(property.Name))
            {
                var prop = new SchemaProperty
                {
                    Class = className,
                    IsSingleValued = property.IsSingleValued,
                    Name = property.Name,
                    IsInGlobalCatalog = property.IsInGlobalCatalog,
                    RangeLower = property.RangeLower,
                    RangeUpper = property.RangeUpper
                };

                this.Add(prop);
                return true;
            }

            return false;
        }

        public void AddFromClass(ActiveDirectorySchemaClass? schemaClass)
        {
            if (schemaClass is null)
                return;

            var collection = schemaClass.GetAllProperties();
            if (!this.TryGetByClassInternal(schemaClass.Name, out UniqueList<SchemaProperty>? list))
            {
                list = CreateNewList(collection.Count);
                _classToProp.Add(schemaClass.Name, list);
            }

            foreach (SchemaProperty sp in EnumerateCollection(schemaClass.Name, collection, _attributeToProp))
            {
                list.Add(sp);
            }
        }

        public bool ContainsClass(string className)
        {
            return _classToProp.ContainsKey(className);
        }

        public IEnumerator<SchemaProperty> GetEnumerator()
        {
            return _attributeToProp.Values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool TryGet(string property, [NotNullWhen(true)] out SchemaProperty? value)
        {
            return _attributeToProp.TryGetValue(property, out value);
        }
        public bool TryGetByClass(string className, [NotNullWhen(true)] out ISearchableList<SchemaProperty>? list)
        {
            list = null;
            if (this.TryGetByClassInternal(className, out UniqueList<SchemaProperty>? value))
            {
                list = value;
            }

            return list is not null;
        }

        private void Add(SchemaProperty schemaProperty)
        {
            _attributeToProp.Add(schemaProperty.Name, schemaProperty);
            if (!_classToProp.TryGetValue(schemaProperty.Class, out UniqueList<SchemaProperty>? list))
            {
                list = CreateNewList(900);
                _classToProp.Add(schemaProperty.Class, list);
            }

            list.Add(schemaProperty);
        }
        private UniqueList<SchemaProperty> CreateNewList(int capacity = 1)
        {
            return new UniqueList<SchemaProperty>(capacity);
        }
        private static IEnumerable<SchemaProperty> EnumerateCollection(string className, ICollection collection, Dictionary<string, SchemaProperty> dict)
        {
            foreach (ActiveDirectorySchemaProperty property in collection)
            {
                using (property)
                {
                    if (dict.TryGetValue(property.Name, out SchemaProperty? value))
                    {
                        yield return value;
                        continue;
                    }

                    var sp = new SchemaProperty
                    {
                        Class = className,
                        IsInGlobalCatalog = property.IsInGlobalCatalog,
                        IsSingleValued = property.IsSingleValued,
                        Name = property.Name,
                        RangeLower = property.RangeLower,
                        RangeUpper = property.RangeUpper
                    };

                    dict.Add(sp.Name, sp);
                    yield return sp;
                }
            }
        }
        private bool TryGetByClassInternal(string className, [NotNullWhen(true)] out UniqueList<SchemaProperty>? list)
        {
            return _classToProp.TryGetValue(className, out list);
        }
    }
}
