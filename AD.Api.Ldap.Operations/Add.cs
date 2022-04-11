using AD.Api.Ldap.Extensions;
using AD.Api.Ldap.Operations.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Operations
{
    public class Add : EditPropertyOperationBase, ILdapOperationWithValues
    {
        public override OperationType OperationType => OperationType.Add;
        [NotNull]
        public override string? Property { get; }
        public List<object> Values { get; }

        public Add(string propertyName)
            : base()
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            this.Property = propertyName;

            this.Values = new List<object>(1);
        }

        public override bool Perform(PropertyValueCollection collection)
        {
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
            this.Values.ForEach(obj => serializer.Serialize(writer, obj));

            writer.WriteEndArray();
        }
    }
}
