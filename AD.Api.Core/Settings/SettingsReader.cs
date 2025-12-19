using System.Text.RegularExpressions;

namespace AD.Api.Core.Settings;

/// <summary>
/// Provides functionality to read and replace environment-based settings within application configuration models.
/// </summary>
/// <remarks>This service enables automatic substitution of environment variable values in configuration objects
/// that implement the <see cref="IEnvironmentSetting{TSelf}"/> interface. It is intended for internal use and is not thread-safe. The
/// service relies on an <see cref="IConfiguration"/> instance to access application settings and environment variables.</remarks>
internal sealed partial class SettingsReaderService : ISettingsReader
{
	private const string ENV_VALUE = "EnvValue";
	private const string APPSETTING_PREFIX = "APPSETTING_";
	[DebuggerHidden, DebuggerStepThrough, GeneratedRegex(@"\{env\:(?'" + ENV_VALUE + @"'.+)\}", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.ExplicitCapture)]
	private static partial Regex GetEnvValueRegex();

	private static readonly Regex s_envValueRegex = GetEnvValueRegex();
	/// <summary>
	/// Initializes a new instance of the <see cref="SettingsReaderService"/> class.
	/// </summary>
	public SettingsReaderService() { }

	public T ReplaceAppEnvironmentSettings<T>(T model) where T : class, IEnvironmentSetting<T>
	{
		Debug.Assert(model is not null, $"{nameof(model)} should not be null.");

		using (var accessors = T.GetAccessors())
		{
			foreach (GetSetString<T> accessor in accessors)
			{
				PerformTranslation(model, accessor);
			}

			return model;
		}
	}

	private static void PerformTranslation<T>(T model, GetSetString<T> accessor) where T : class
	{
		string? value = accessor.GetValue(model);
		if (!TryGetMatch(value, out Group? group))
		{
			return;
		}

		string? resolvedValue = ResolveEnvironmentValue(group);
		accessor.SetValue(model, resolvedValue);
	}

	private static string? ResolveEnvironmentValue(Group captureGroup)
	{
		string variableName = string.Concat(APPSETTING_PREFIX, captureGroup.ValueSpan.Trim());
		return Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process);
	}

	[DebuggerStepThrough]
	private static bool TryGetMatch([NotNullWhen(true)] string? value, [NotNullWhen(true)] out Group? group)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			group = null;
			return false;
		}

		Match match = s_envValueRegex.Match(value);
		if (!match.Success || match.Groups.Count < 2)
		{
			group = null;
			return false;
		}

		group = match.Groups[ENV_VALUE];
		return group.Success;
	}
}

public static class SettingsReaderDependencyInjection
{
	/// <summary>
	/// Adds a singleton implementation of <see cref="ISettingsReader"/> to the service collection and returns the created
	/// instance via an output parameter.
	/// </summary>
	/// <param name="services">The service collection to which the <see cref="ISettingsReader"/> implementation will be added.</param>
	/// <param name="reader">When this method returns, contains the singleton instance of <see cref="ISettingsReader"/> that was registered.</param>
	/// <returns>The <see cref="IServiceCollection"/> with the <see cref="ISettingsReader"/> service registered as a singleton.</returns>
	public static IServiceCollection AddSettingsReader(this IServiceCollection services, out ISettingsReader reader)
	{
		return services.AddSingleton(reader = new SettingsReaderService());
	}
}