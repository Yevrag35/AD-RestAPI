using AD.Api.Ldap.Exceptions;
using AD.Api.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.DirectoryServices;

namespace AD.Api.Ldap.Operations
{
    public class Replace : EditPropertyOperationBase
    {
        protected IEqualityComparer<object> Comparer { get; }
        public override OperationType OperationType => OperationType.Replace;

        public override string Property { get; }
        public virtual List<(object Original, object New)> Values { get; }

        public Replace(string propertyName, IEqualityComparer<object>? comparer = null)
            : base()
        {
            if (comparer is null)
                comparer = (IEqualityComparer<object>)StringComparer.CurrentCultureIgnoreCase;

            this.Comparer = comparer;

            this.Property = propertyName;
            this.Values = new List<(object, object)>(1);
        }

        public override bool Perform(PropertyValueCollection collection, SchemaProperty schemaProperty)
        {
            this.HasBeenPerformed = true;
            if (!schemaProperty.IsSingleValued)
            {
                this.Values.Clear();
                return false;
            }

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
                    (object? Original, object? New) = this.Values[n];
                    if (this.Comparer.Equals(item, Original))
                    {
                        collection.Insert(i, New);
                        collection.Remove(item);
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

            this.Values.ForEach(obj =>
            {
                string originalName = obj.Original.ToString()
                    ?? throw new LdapOperationParsingException($"The property name in JSON cannot be null or empty.", OperationType.Replace);

                writer.WritePropertyName(originalName);
                writer.WriteValue(obj.New.ToString());
            });

            writer.WriteEndObject();
        }
    }
}
