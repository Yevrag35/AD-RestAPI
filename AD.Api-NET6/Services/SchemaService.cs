using AD.Api.Ldap;
using AD.Api.Ldap.Attributes;
using AD.Api.Schema;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices.ActiveDirectory;

namespace AD.Api.Services
{
    public interface ISchemaService
    {
        SchemaCache Dictionary { get; }
        ILdapEnumDictionary Enums { get; }

        bool HasAllAttributesCached(IEnumerable<string> keys, [NotNullWhen(false)] out List<string>? missing);
        void LoadAttributes(IEnumerable<string> attributes, LdapConnection connection);

        //bool IsClassLoaded(string? className);
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="className"></param>
        ///// <param name="domain"></param>
        ///// <exception cref="ArgumentNullException"/>
        ///// <exception cref="InvalidOperationException"/>
        //void LoadClass(string className, string? domain = null);

        //bool TryGet(string? attributeName, [NotNullWhen(true)] out SchemaProperty? property);
    }

    public class SchemaService : ISchemaService
    {
        public SchemaCache Dictionary { get; }
        public ILdapEnumDictionary Enums { get; }

        public SchemaService(SchemaCache schemaCache, ILdapEnumDictionary enumDictionary)
        {
            this.Dictionary = schemaCache;
            this.Enums = enumDictionary;
        }

        public bool HasAllAttributesCached(IEnumerable<string> keys, [NotNullWhen(false)] out List<string>? missing)
        {
            missing = null;
            var lazyList = new Lazy<List<string>>();
            foreach (string key in keys)
            {
                if (!this.Dictionary.ContainsKey(key))
                {
                    lazyList.Value.Add(key);
                }
            }

            if (lazyList.IsValueCreated)
                missing = lazyList.Value;

            return !lazyList.IsValueCreated;
        }

        public void LoadAttributes(IEnumerable<string> attributes, LdapConnection connection)
        {
            var ctx = connection.GetForestContext();
            using (var schema = ActiveDirectorySchema.GetSchema(ctx))
            {
                foreach (string name in attributes)
                {
                    using (var property = schema.FindProperty(name))
                    {
                        this.Dictionary.Add(property);
                    }
                }
            }
        }
    }
}
