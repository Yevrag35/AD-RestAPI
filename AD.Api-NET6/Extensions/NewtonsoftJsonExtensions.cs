using AD.Api.Ldap.Converters;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Collections.Generic;

namespace AD.Api.Extensions
{
    public static class NewtonsoftJsonExtensions
    {
        public static MvcNewtonsoftJsonOptions AddADApiConfiguration(this MvcNewtonsoftJsonOptions options)
        {
            options.SerializerSettings.Converters.AddADApiConverters();
            options.SerializerSettings.Converters.Add(new ProxyAddressConverter());

            options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
            options.SerializerSettings.Formatting = Formatting.Indented;
            options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
            options.SerializerSettings.MissingMemberHandling = MissingMemberHandling.Ignore;

            return options;
        }

        public static void AddADApiConverters(this IList<JsonConverter> list, NamingStrategy? namingStrategy = null)
        {
            bool hasFilterConverter = false;
            bool hasPathValueConverter = false;
            bool hasStringEnumConverter = false;
            bool hasOperationConverter = false;

            CheckConverters(list, ref hasFilterConverter, ref hasPathValueConverter, ref hasStringEnumConverter, ref hasOperationConverter);

            if (!hasFilterConverter)
                list.Add(new FilterConverter());

            if (!hasPathValueConverter)
                list.Add(new PathValueJsonConverter());

            if (!hasOperationConverter)
                list.Add(new EditOperationJsonConverter());

            if (!hasStringEnumConverter)
            {
                if (namingStrategy is null)
                    namingStrategy = new DefaultNamingStrategy();

                list.Add(new StringEnumConverter(namingStrategy));
            }
        }

        private static void CheckConverters(IList<JsonConverter> list, 
            ref bool hasFilterConverter, ref bool hasPathValueConverter, ref bool hasStringEnumConverter, ref bool hasOperationConverter)
        {
            foreach (JsonConverter converter in list)
            {
                if (converter is FilterConverter)
                    hasFilterConverter = true;

                else if (converter is PathValueJsonConverter)
                    hasPathValueConverter = true;

                else if (converter is StringEnumConverter)
                    hasStringEnumConverter = true;

                else if (converter is EditOperationJsonConverter)
                    hasOperationConverter = true;
            }
        }
    }
}
