using AD.Api.Schema;

namespace AD.Api.Extensions
{
    public static class LoadDefaultSchemaAttributesExtension
    {
        private static readonly Action<BinderOptions> _binderOptions = options => options.ErrorOnUnknownConfiguration = true;

        public static IServiceCollection AddDefaultSchemaAttributes(this IServiceCollection services, IConfigurationSection configurationSection)
        {
            SchemaCache cache = new(ReadAttributesFromConfig(configurationSection.GetChildren()));

            return services.AddSingleton(cache);
        }

        private static IEnumerable<SchemaProperty> ReadAttributesFromConfig(IEnumerable<IConfigurationSection> items)
        {
            foreach (IConfigurationSection item in items)
            {
                yield return item.Get<SchemaProperty>(_binderOptions);
            }
        }
    }
}
