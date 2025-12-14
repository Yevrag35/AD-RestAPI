namespace AD.Api.Startup;

internal static class WebBuilderExtensions
{
	internal static WebApplicationBuilder SetupEnvironment(this WebApplicationBuilder builder)
	{
		builder.Configuration.AddEnvironmentVariables();

		SetupProviderValidation(builder.Host);

		AddJsonSettingsFile(builder);

		return builder;
	}
	private static void AddJsonSettingsFile(WebApplicationBuilder builder)
	{
		string fileName = GetAppSettingsName(builder.Environment);

		string path = Path.Combine(builder.Environment.ContentRootPath, fileName);

		builder.Configuration.AddJsonFile(path, optional: false, reloadOnChange: false);
	}
	private static string GetAppSettingsName(IWebHostEnvironment environment)
	{
		return environment.IsDevelopment()
			? "appsettings.Development.json"
			: "appsettings.json";
	}
	private static void SetupProviderValidation(IHostBuilder hostBuilder)
	{
		hostBuilder.UseDefaultServiceProvider((context, options) =>
		{
			bool isDevelopment = context.HostingEnvironment.IsDevelopment();

			options.ValidateScopes = isDevelopment;
			options.ValidateOnBuild = isDevelopment;
		});
	}
}
