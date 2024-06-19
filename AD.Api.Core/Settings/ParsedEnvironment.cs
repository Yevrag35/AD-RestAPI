using Microsoft.Extensions.Hosting;

namespace AD.Api.Core.Settings
{
    public sealed class ParsedEnvironment
    {
        public const string ProductionJsonFileName = "appsettings.json";

        public string EnvironmentName { get; }
        public string EnvironmentVariableName { get; }
        public string JsonName { get; }

        public ParsedEnvironment(scoped ReadOnlySpan<char> environmentName, string variableName)
        {
            this.EnvironmentVariableName = variableName;
            this.EnvironmentName = environmentName.ToString();
            if (environmentName.Equals(Environments.Production, StringComparison.OrdinalIgnoreCase))
            {
                this.JsonName = ProductionJsonFileName;
            }
            else
            {
                this.JsonName = $"appsettings.{this.EnvironmentName}.json";
            }
        }
    }
}

