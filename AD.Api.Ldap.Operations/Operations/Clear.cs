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
    public class Clear : EditPropertyOperationBase
    {
        public override OperationType OperationType => OperationType.Clear;

        public override string Property { get; }

        public Clear(string propertyName)
        {
            this.Property = propertyName;
        }

        public override bool Perform(PropertyValueCollection collection, SchemaProperty schemaProperty)
        {
            this.HasBeenPerformed = true;
            if (schemaProperty.IsSingleValued)
                collection.Value = null;

            else
                collection.Clear();

            return true;
        }

        public override void WriteTo(JsonWriter writer, NamingStrategy namingStrategy, JsonSerializer serializer)
        {
            string name = namingStrategy.GetPropertyName(this.Property, false);
            writer.WriteValue(name);
        }
    }
}
