namespace AD.Api.Core.Settings;

/// <summary>
/// Provides functionality to read and replace environment-based settings within application configuration models.
/// </summary>
public interface ISettingsReader
{
	/// <summary>
	/// Replaces environment-specific settings in the specified model using the application's current environment
	/// configuration.
	/// </summary>
	/// <remarks>This method iterates over all environment-related properties defined by the model type and applies
	/// the application's environment configuration to each. The model instance is modified in place; no new instance is
	/// created.</remarks>
	/// <typeparam name="T">The type of the model that implements environment setting replacement functionality.</typeparam>
	/// <param name="model">The model instance whose environment settings will be replaced. Must implement <see
	/// cref="IEnvironmentSetting{TSelf}"/>.</param>
	/// <returns>The same model instance with its environment settings updated to match the application's environment.</returns>
	T ReplaceAppEnvironmentSettings<T>(T model) where T : class, IEnvironmentSetting<T>;
}
