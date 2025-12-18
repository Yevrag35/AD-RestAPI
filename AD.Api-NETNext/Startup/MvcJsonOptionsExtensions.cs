using AD.Api.Collections.Extensions;
using AD.Api.Core.Ldap;
using AD.Api.Core.Operations;
using AD.Api.Core.Serialization;
using AD.Api.Core.Serialization.Json.Converters;
using AD.Api.Core.Serialization.Json.Converters.Ldap;
using AD.Api.Serialization.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace AD.Api;

public static class MvcJsonOptionsExtensions
{
	public static IMvcBuilder AddApiControllers(this WebApplicationBuilder appBuilder, IConfigurationSection settingsSection, PropertyConverter converter, Func<WebApplicationBuilder, IMvcBuilder> addControllers)
	{
		bool isDevelopment = appBuilder.Environment.IsDevelopment();
		SerializationSettings settings = GetSerializationSettings(appBuilder.Services, settingsSection);
		LdapEnumConverter enumConverter = ConfigureAndAddEnumConverter(appBuilder, settings);

		return addControllers(appBuilder)
			.AddJsonOptions(options =>
			{
				WorkingNamingPolicy policy = new(options.JsonSerializerOptions);

				options.AllowInputFormatterExceptionMessages = isDevelopment;
				options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
				options.JsonSerializerOptions.NumberHandling = JsonNumberHandling.AllowReadingFromString;
				options.JsonSerializerOptions.PropertyNamingPolicy = JsonSpanCamelCaseNamingPolicy.SpanPolicy;
				options.JsonSerializerOptions.WriteIndented = settings.WriteIndented;

				options.JsonSerializerOptions.Converters.Add(enumConverter);
				AddAdditionalJsonConverters(options.JsonSerializerOptions, converter, policy);

				options.JsonSerializerOptions.TypeInfoResolver = new DefaultJsonTypeInfoResolver
				{
					Modifiers = { PrivateExtensionDataModifier.AddPrivateExtensionData }
				};
			});
	}

	private static void AddAdditionalJsonConverters(JsonSerializerOptions options, PropertyConverter converter, WorkingNamingPolicy policy)
	{
		options.Converters.AddMany(
			new UserAccountControlConverter(),
			new ClearOperationConverter(policy),
			new DistinguishedNameConverter(),
			new KeyValuePairArrayConverter<DistinguishedName>(policy) { IsOrdered = true },
			new OneEditOperationConverter<AddDictionary>(policy),
			new OneEditOperationConverter<RemoveDictionary>(policy),
			new OneEditOperationConverter<SetDictionary>(policy),
			new RelativeNameConverter(),
			new ReplaceOperationConverter(policy),
			new ResultEntryConverter(converter),
			new ResultEntryCollectionConverter(converter),
			new SidStringConverter(),
			new StringValuesAsStringConverter());
	}
	private static LdapEnumConverter ConfigureAndAddEnumConverter(IHostApplicationBuilder appBuilder, SerializationSettings settings)
	{
		var enumConverter = LdapEnumConverter.Create(settings, (options, state) =>
		{
			options.SetNamingPolicy(JsonSpanCamelCaseNamingPolicy.SpanPolicy)
				   .SerializeFlagsAsArray(state.WriteEnumFlagsAsArray)
				   .Exclude<GroupType>()
				   .Exclude<SamAccountType>()
				   .Exclude<UserAccountControl>();
		});

		_ = appBuilder.Services.AddSingleton<LdapEnumConverter>(enumConverter);
		return enumConverter;
	}

	private static SerializationSettings GetSerializationSettings(IServiceCollection services, IConfiguration configuration)
	{
		IConfigurationSection section = configuration
			.GetSection("Serialization");

		SerializationSettings? settings = section.Get<SerializationSettings>()
			?? new SerializationSettings
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

		services.AddSingleton(settings);
		return settings;
	}
}
