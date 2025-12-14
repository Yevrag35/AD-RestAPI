using AD.Api.Core.Serialization;
using AD.Api.Serialization.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD.Api.Startup;

internal static class HttpJsonOptionsExtensions
{
	internal static IServiceCollection AddJsonWithConverters(this IServiceCollection services)
	{
		services.ConfigureHttpJsonOptions(options => ConfigureJsonOptions(options.SerializerOptions));
		services.AddJsonOptions();

		return services;
	}

	static void ConfigureJsonOptions(JsonSerializerOptions options)
	{
		options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		options.PropertyNamingPolicy = JsonSpanCamelCaseNamingPolicy.SpanPolicy;
		options.PropertyNameCaseInsensitive = true;
		options.ReferenceHandler = ReferenceHandler.IgnoreCycles;

		WorkingNamingPolicy policy = new(options);


		//options.Converters.AddSingleton<DictionaryElementConverter>()
		//				  .AddSingletonWithPolicy<KeyValuePairConverter>(policy)
		//				  .AddSingletonWithPolicy<CmdletNameResponseConverter>(policy)
		//				  .AddSingleton<ListElementConverter>()
		//				  .AddSingleton<NIndexConverter>()
		//				  .AddSingleton<RequestHeadersConverter>()
		//				  .AddSingleton<StringValuesConverter>()
		//				  .AddSingletonWithPolicy<SyncListConverterFactory>(policy)
		//				  .AddMany(
		//					new CmdletParameterDictionaryConverter(policy),
		//					new CompactDictionaryConverter(policy, additionalCapacityOnDeserialization: 2), // 2 extra properties for ExternalDirectoryObjectId and FoundInExo
		//					new IdDictionaryConverter(ExoConstants.ExternalDirectoryObjectId, policy, additionalCapacityOnDeserialization: 2),
		//					new ImmutableArrayConverter<string>(),
		//					new JsonStringEnumConverter(namingPolicy: policy));
	}
}
