using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AD.Api.Extensions
{
    public static class JsonNetExtensions
    {
        public static IMvcBuilder ConfigureJson(this WebApplicationBuilder builder, Func<WebApplicationBuilder, IMvcBuilder> addControllers, Action<IConfigurationRoot, IWebHostEnvironment, JsonOptions> configureOptions)
        {
            
            return addControllers.Invoke(builder)
                .AddJsonOptions(options =>
                {
                    configureOptions.Invoke(builder.Configuration, builder.Environment, options);
                });
        }
    }
}