using AD.Api.Ldap.Extensions;
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
    public class Set : EditPropertyOperationBase
    {
        public override OperationType OperationType => OperationType.Set;

        public override string Property { get; }
        public IList<object> Values { get; }

        public Set(string propertyName)
        {
            this.Property = propertyName;
            this.Values = new List<object>(1);
        }

        public override bool Perform(PropertyValueCollection collection)
        {
            collection.Clear();
            this.HasBeenPerformed = true;

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

            switch (this.Values.Count)
            {
                case > 1:
                {
                    writer.WriteStartArray();
                    this.Values.ForEach(obj =>
                    {
                        writer.WriteValue(obj);
                    });

                    writer.WriteEndArray();
                    break;
                }

                default:
                    writer.WriteValue(this.Values[0]);
                    break;
            }
        }
    }

    
}
