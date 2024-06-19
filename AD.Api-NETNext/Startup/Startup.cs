using AD.Api.Core.Extensions;
using AD.Api.Core.Settings;

namespace AD.Api
{
    internal static class StartupHelper
    {
        internal static WebApplicationBuilder CreateWebBuilder(string[] arguments)
        {
            Span<string> argSpan = arguments.AsSpan();
            ParsedEnvironment environment = EnvironmentExtensions.UseEnvironmentFromArgs(ref argSpan);

            if (argSpan.Length != arguments.Length)
            {
                arguments = argSpan.ToArray();
            }

            var builder = WebApplication.CreateBuilder(new WebApplicationOptions
            {
                Args = arguments,
                ContentRootPath = AppDomain.CurrentDomain.BaseDirectory,
                EnvironmentName = environment.EnvironmentName,
            });

            builder.Configuration.AddEnvironmentVariables()
                                 .AddJsonFile(environment.JsonName);

            Environment.SetEnvironmentVariable("AppEnvName", environment.EnvironmentName);

            return builder;
        }
    }
}
