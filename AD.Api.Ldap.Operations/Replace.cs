using AD.Api.Ldap.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;

namespace AD.Api.Ldap.Operations
{
    public class Replace : EditPropertyOperationBase
    {
        private readonly IEqualityComparer<object> _comparer;
        public override OperationType OperationType => OperationType.Replace;

        public override string Property { get; }
        public IList<(object Original, object New)> Values { get; }

        public Replace(string propertyName, IEqualityComparer<object>? comparer = null)
            : base()
        {
            if (comparer is null)
                comparer = (IEqualityComparer<object>)StringComparer.CurrentCultureIgnoreCase;

            _comparer = comparer;

            this.Property = propertyName;
            this.Values = new List<(object, object)>(1);
        }

        public override bool Perform(PropertyValueCollection collection)
        {
            this.HasBeenPerformed = true;
            int valCount = this.Values.Count;
            int colCount = collection.Count;
            if (colCount <= 0)
                return false;

            else
                colCount--;

            if (valCount <= 0)
                return true;

            for (int i = colCount; i >= 0; i--)
            {
                object? item = collection[i];

                for (int n = 0; n < this.Values.Count; n++)
                {
                    object? val = this.Values[i];
                    if (_comparer.Equals(item, val))
                    {
                        collection.Remove(item);
                        collection.Add(val);
                    }
                }
            }

            this.Values.Clear();

            return collection.Count == colCount + 1;
        }

        public override void WriteTo(JsonWriter writer, NamingStrategy namingStrategy, JsonSerializer serializer)
        {
            string name = namingStrategy.GetPropertyName(this.Property, false);
            writer.WritePropertyName(name);

            writer.WriteStartObject();
            writer.WritePropertyName(namingStrategy.GetPropertyName(nameof(Replace), false));

            writer.WriteStartObject();
            this.Values.ForEach(obj =>
            {
                writer.WritePropertyName(obj.Original.ToString());
                writer.WriteValue(obj.New.ToString());
            });

            writer.WriteEndObject();
            writer.WriteEndObject();
        }
    }
}
