using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;

namespace AD.Api.Schema
{
    public static class SchemaCache
    {
        private readonly static StringComparer _ignoreCase = StringComparer.CurrentCultureIgnoreCase;
        private readonly static Lazy<Dictionary<string, Dictionary<string, SchemaProperty>>> _schemaDictionary = new(InitCache);

        public static bool IsLoaded => _schemaDictionary.IsValueCreated;

        //public static void LoadSchema(DirectoryContext ctx, string[] classNames)
        //{
        //    if (classNames is null || classNames.Length <= 0)
        //        throw new ArgumentNullException(nameof(classNames));

        //    using ActiveDirectorySchema schema = ActiveDirectorySchema.GetSchema(ctx);
        //    Array.ForEach(classNames, name =>
        //    {
        //        var propDict = GetClassDictionary(schema, name);
        //        if (propDict.Count > 0)
        //        {
        //            _schemaDictionary.Value.Add(name, propDict);
        //        }
        //    });
        //}

        //private static Dictionary<string, SchemaProperty> GetClassDictionary(ActiveDirectorySchema schema, string className)
        //{
        //    using ActiveDirectorySchemaClass schemaClass = schema.FindClass(className);
        //    IEnumerable<SchemaProperty> allPropertiesFromClass = GetAllProperties(schemaClass);
        //    return allPropertiesFromClass.ToDictionary(x => x.Name, _ignoreCase);
        //}

        //private static IEnumerable<SchemaProperty> GetAllProperties(ActiveDirectorySchemaClass schemaClass)
        //{
        //    var col = schemaClass.GetAllProperties();
        //    foreach (ActiveDirectorySchemaProperty prop in col)
        //    {
        //        using (prop)
        //        {
        //            //yield return SchemaProperty.FromProperty(prop, schemaClass.Name);
        //        }
        //    }
        //}

        private static Dictionary<string, Dictionary<string, SchemaProperty>> InitCache()
        {
            return new Dictionary<string, Dictionary<string, SchemaProperty>>(5, _ignoreCase);
        }
    }
}