using AD.Api.Ldap.Properties;
using AD.Api.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.DirectoryServices;

namespace AD.Api.Ldap.Operations
{
    public sealed class ProxyAddressesOperation : EditPropertyOperationBase
    {
        internal const string PROPERTY = "ProxyAddresses";
        private ProxyAddressCollection? _original;

        public sealed override OperationType OperationType { get; }
        public sealed override string Property => PROPERTY;
        public ProxyAddressCollection Values { get; }

        public ProxyAddressesOperation(OperationType operationType)
        {
            this.OperationType = operationType;
            this.Values = new ProxyAddressCollection();
        }
        public ProxyAddressesOperation(OperationType operationType, IEnumerable<string> addresses)
        {
            this.OperationType = operationType;
            this.Values = new ProxyAddressCollection(addresses);
        }

        public sealed override bool Perform(PropertyValueCollection collection, SchemaProperty schemaProperty)
        {
            _original = new ProxyAddressCollection(collection);
            collection.Clear();

            return this.OperationType switch
            {
                OperationType.Add => PerformAdd(collection, _original, this.Values),
                OperationType.Remove => PerformRemove(collection, _original, this.Values),
                OperationType.Set => PerformSet(collection, this.Values),
                _ => false,
            };
        }

        private static bool PerformAdd(PropertyValueCollection collection, ProxyAddressCollection original, ProxyAddressCollection additions)
        {
            Action<ProxyAddress> action;
            bool addHavePrim = additions.HasPrimary();

            if (addHavePrim)
            {
                action = pa =>
                {
                    pa.IsPrimary = false;
                    collection.Add(pa.GetValue());
                };
            }
            else
                action = pa => collection.Add(pa.GetValue());

            original.ForEach(action);
            additions.ForEach(pa => collection.Add(pa.GetValue()));

            return collection.Count == original.Count + additions.Count;
        }
        private static bool PerformSet(PropertyValueCollection collection, ProxyAddressCollection replacements)
        {
            replacements.ForEach(pa =>
            {
                collection.Add(pa.GetValue());
            });

            return collection.Count == replacements.Count;
        }
        private static bool PerformRemove(PropertyValueCollection collection, ProxyAddressCollection original, ProxyAddressCollection removals)
        {
            if (!original.Overlaps(removals))
                return false;

            if (removals.HasPrimary() && original.HasPrimary())
            {
                original.Find(x => !x.IsPrimary && !removals.Contains(x)).IsPrimary = true;
            }

            original.ForEach(pa =>
            {
                if (!removals.Contains(pa))
                    collection.Add(pa.GetValue());
            });

            return collection.Count == original.Count - removals.Count;
        }

        public sealed override void WriteTo(JsonWriter writer, NamingStrategy namingStrategy, JsonSerializer serializer)
        {
            string name = namingStrategy.GetPropertyName(PROPERTY, false);
            writer.WritePropertyName(name);

            serializer.Serialize(writer, this.Values);
        }
    }
}
