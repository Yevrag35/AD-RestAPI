using AD.Api.Ldap.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;

namespace AD.Api.Ldap.Operations
{
    public class ProxyAddressesOperation : EditPropertyOperationBase, IDisposable
    {
        internal const string PROPERTY = "ProxyAddresses";
        private ProxyAddressCollection? _original;
        private bool _disposed;
        public override OperationType OperationType { get; }
        public override string Property => PROPERTY;
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

        #region IDISPOSABLE IMPLEMENTATION
        public void Dispose()
        {
            if (_disposed)
                return;

            this.Values.Dispose();
            this.Values.Clear();
            _original?.Dispose();
            _original?.Clear();

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        public override bool Perform(PropertyValueCollection collection)
        {
            _original = new ProxyAddressCollection(collection);
            collection.Clear();

            switch (this.OperationType)
            {
                case OperationType.Add:
                    return PerformAdd(collection, _original, this.Values);

                case OperationType.Remove:
                    return PerformRemove(collection, _original, this.Values);

                case OperationType.Set:
                    return PerformSet(collection, this.Values);

                default:
                    return false;
            }
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

            original.ForEach(pa =>
            {
                if (!removals.Contains(pa))
                    collection.Add(pa.GetValue());
            });

            return collection.Count == original.Count - removals.Count;
        }

        public override void WriteTo(JsonWriter writer, NamingStrategy namingStrategy, JsonSerializer serializer)
        {
            string name = namingStrategy.GetPropertyName(PROPERTY, false);
            writer.WritePropertyName(name);

            serializer.Serialize(writer, this.Values);
        }

        #endregion
    }
}
