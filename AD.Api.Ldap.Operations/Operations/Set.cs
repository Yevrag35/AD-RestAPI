using AD.Api.Ldap.Extensions;
using AD.Api.Ldap.Operations.Internal;
using AD.Api.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Operations
{
    public class Set : EditPropertyOperationBase, ILdapOperationWithValues
    {
        public override OperationType OperationType => OperationType.Set;

        public override string Property { get; }
        public List<object> Values { get; }

        public Set(string propertyName)
        {
            this.Property = propertyName;
            this.Values = new List<object>(1);
        }

        public override bool Perform(PropertyValueCollection collection, SchemaProperty schemaProperty)
        {
            collection.Clear();
            this.HasBeenPerformed = true;
            if (schemaProperty.IsSingleValued && this.Values.Count >= 1)
                return this.Modify(collection, schemaProperty, this.Values[0]);

            int i = -1;
            foreach (object item in this.Values)
            {
                i = collection.Add(item);
            }

            if (i > -1)
            {
                this.Values.Clear();
                return true;
            }
            else
                return false;
        }

        public override void WriteTo(JsonWriter writer, NamingStrategy namingStrategy, JsonSerializer serializer)
        {
            string name = namingStrategy.GetPropertyName(this.Property, false);
            writer.WritePropertyName(name);

            writer.WriteStartArray();
            this.Values.ForEach(obj =>
            {
                serializer.Serialize(writer, obj);
            });

            writer.WriteEndArray();
        }
    }
}
