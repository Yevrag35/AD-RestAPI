using AD.Api.Collections.Extensions;
using AD.Api.Core.Ldap;
using AD.Api.Core.Operations;
using AD.Api.Core.Serialization;
using AD.Api.Core.Serialization.Json.Converters;
using AD.Api.Core.Serialization.Json.Converters.Ldap;
using AD.Api.Serialization.Json;
using System.Text.Json.Serialization.Metadata;

namespace AD.Api.Startup;

internal static class HttpJsonOptionsExtensions
{
	internal static IServiceCollection AddJsonWithConverters(this IServiceCollection services, IConfiguration serializationConfiguration, PropertyConverter propertyConverter)
	{
		SerializationSettings settings = GetSerializationSettings(services, serializationConfiguration);

		services.ConfigureHttpJsonOptions(options => ConfigureJsonOptions(options.SerializerOptions, propertyConverter, settings));
		services.AddJsonOptions();

		return services;
	}

	static void ConfigureJsonOptions(JsonSerializerOptions options, PropertyConverter propertyConverter, SerializationSettings settings)
	{
		var enumConverter = LdapEnumConverter.Create(settings, (options, state) =>
		{
			options.SetNamingPolicy(JsonSpanCamelCaseNamingPolicy.SpanPolicy)
				   .SerializeFlagsAsArray(state.WriteEnumFlagsAsArray)
				   .Exclude<GroupType>()
				   .Exclude<SamAccountType>()
				   .Exclude<UserAccountControl>();
		});

		options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
		options.PropertyNamingPolicy = JsonSpanCamelCaseNamingPolicy.SpanPolicy;
		options.PropertyNameCaseInsensitive = true;
		options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
		options.WriteIndented = settings.WriteIndented;

		options.TypeInfoResolver = new DefaultJsonTypeInfoResolver
		{
			Modifiers = { PrivateExtensionDataModifier.AddPrivateExtensionData },
		};

		WorkingNamingPolicy policy = new(options);

		options.Converters.AddMany(
			enumConverter,
			new UserAccountControlConverter(),
			new ClearOperationConverter(policy),
			new DistinguishedNameConverter(),
			new KeyValuePairArrayConverter<DistinguishedName>(policy) { IsOrdered = true },
			new OneEditOperationConverter<AddDictionary>(policy),
			new OneEditOperationConverter<RemoveDictionary>(policy),
			new OneEditOperationConverter<SetDictionary>(policy),
			new RelativeNameConverter(),
			new ReplaceOperationConverter(policy),
			new ResultEntryConverter(propertyConverter),
			new ResultEntryCollectionConverter(propertyConverter),
			new SidStringConverter(),
			new StringValuesAsStringConverter());
	}
	private static SerializationSettings GetSerializationSettings(IServiceCollection services, IConfiguration configuration)
	{
		IConfigurationSection section = configuration.GetSection("Serialization");

		if (!section.Exists() || section.Get<SerializationSettings>() is not SerializationSettings settings)
		{
			settings = new SerializationSettings
			{
				DateTimeAttributes = [
					"accountExpires",
						"badPasswordTime",
						"lastLogon",
						"lastLogonTimestamp",
						"lockoutTime",
						"pwdLastSet",
						"whenChanged",
						"whenCreated",
				],
				GuidAttributes = [
					"ms-DS-ConsistencyGuid",
						"objectGUID",
				],
				WriteEnumFlagsAsArray = false,
				WriteIndented = true,
				WriteSimpleObjectClass = true,
			};
		}

		services.AddSingleton(settings);
		return settings;
	}
}
