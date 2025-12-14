using AD.Api.Serialization.Json;

namespace AD.Api.Core.Serialization;

internal sealed class ApiJsonOptions : IJsonOptions
{
	public JsonSerializerOptions SerializerOptions { get; }

	public ApiJsonOptions(JsonSerializerOptions options)
	{
		this.SerializerOptions = options;
	}
}

/// <summary>
/// Provides extension methods for registering JSON serialization options in the dependency injection container.
/// </summary>
/// <remarks>Use this class to add and configure application-wide JSON serialization settings by registering an
/// implementation of <see cref="IJsonOptions"/>. The configured options can be injected wherever <see
/// cref="IJsonOptions"/> is required, enabling consistent serialization behavior throughout the application.</remarks>
public static class JsonOptionsDependencyInjection
{
	/// <summary>
	/// Adds a singleton implementation of <see cref="IJsonOptions"/> to the service collection, using the specified
	/// factory to configure <see cref="JsonSerializerOptions"/>.
	/// </summary>
	/// <remarks>Use this method to customize JSON serialization settings application-wide by providing a factory
	/// for <see cref="JsonSerializerOptions"/>. The configured options will be available via dependency injection wherever
	/// <see cref="IJsonOptions"/> is injected.</remarks>
	/// <param name="services">The <see cref="IServiceCollection"/> to which the <see cref="IJsonOptions"/> service will be added.</param>
	/// <param name="getOptions">A factory function that receives the current <see cref="IServiceProvider"/> and returns a configured <see
	/// cref="JsonSerializerOptions"/> instance.</param>
	/// <returns>The original <see cref="IServiceCollection"/> instance with the <see cref="IJsonOptions"/> service registered.</returns>
	public static IServiceCollection AddJsonOptions(this IServiceCollection services)
	{
		return services
			.AddSingleton<WorkingNamingPolicy>(p => new(p.GetRequiredService<IJsonOptions>().SerializerOptions))
			.AddSingleton<IJsonOptions>(p => p.GetRequiredService<ApiJsonOptions>())
			.AddSingleton<ApiJsonOptions>(p =>
			{
				JsonSerializerOptions? options = p.GetService<IOptions<Microsoft.AspNetCore.Http.Json.JsonOptions>>()?.Value.SerializerOptions;
				if (options is null)
				{
					// fallback to MVC options if Minimal API options are not available
					options = p.GetRequiredService<IOptions<Microsoft.AspNetCore.Mvc.JsonOptions>>().Value.JsonSerializerOptions;
				}

				return new(new(options));
			});
	}
}
