using AD.Api.Ldap.Converters;
using AD.Api.Settings;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace AD.Api.Extensions
{
    public static class NewtonsoftJsonExtensions
    {
        public static MvcNewtonsoftJsonOptions AddADApiConfiguration(this MvcNewtonsoftJsonOptions options, ITextSettings textSettings)
        {
            options.SerializerSettings.Converters.Add(new EditOperationJsonConverter(textSettings.LdapEditNamingStrategy));
            options.SerializerSettings.Converters.Add(new FilterConverter(textSettings.LdapPropertyNamingStrategy));
            options.SerializerSettings.Converters.Add(new PathValueJsonConverter());
            options.SerializerSettings.Converters.Add(new ProxyAddressConverter());
            options.SerializerSettings.Converters.Add(new StringEnumConverter(textSettings.StringEnumNamingStrategy));

            options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            options.SerializerSettings.DateTimeZoneHandling = textSettings.DateTimeZoneHandling;
            options.SerializerSettings.Formatting = Formatting.Indented;
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

            return options;
        }
    }
}
