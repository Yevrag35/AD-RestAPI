using AD.Api.Ldap;
using AD.Api.Schema;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices.ActiveDirectory;

namespace AD.Api.Services
{
    public interface ISchemaService
    {
        SchemaCache Dictionary { get; }

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
        private IConnectionService Connections { get; }

        public SchemaService(IConnectionService connectionService)
        {
            this.Connections = connectionService;
            this.Dictionary = new SchemaCache();
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

        //public bool IsClassLoaded(string? className)
        //{
        //    if (string.IsNullOrWhiteSpace(className))
        //        return false;

        //    return this.Dictionary.ContainsClass(className);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="className"></param>
        /// <param name="domain"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        //public void LoadClass(string className, string? domain = null)
        //{
        //    if (string.IsNullOrWhiteSpace(className))
        //        throw new ArgumentNullException(nameof(className));

        //    using var connection = this.Connections.GetConnection(domain);
        //    var context = connection.GetForestContext();

        //    using var schema = ActiveDirectorySchema.GetSchema(context);
        //    using (var schemaClass = schema.FindClass(className))
        //    {
        //        //this.Dictionary.AddFromClass(schemaClass);
        //    }
        //    //ActiveDirectorySchemaClass schemaClass;
        //    //try 
        //    //{
        //    //    schemaClass = schema.FindClass(className);
        //    //}
        //    //catch (Exception ex)
        //    //{
        //    //    schema.Dispose();
        //    //    connection.Dispose();
        //    //    throw new InvalidOperationException(ex.Message, ex);
        //    //}
        //}

        //public bool TryGet(string? attributeName, [NotNullWhen(true)] out SchemaProperty? property)
        //{
        //    return this.Dictionary.TryGet(attributeName ?? string.Empty, out property);
        //}
    }
}
