namespace AD.Api;

internal static class WebEnv
{
#if DEBUG
	internal static readonly bool IsDebugBuild = true;
	internal static readonly string EnvironmentName = Microsoft.Extensions.Hosting.Environments.Development;
	internal static readonly bool SerializeIndent = EnvironmentWantsIndents();
#else
    internal static readonly bool IsDebugBuild = false;
    internal static readonly string EnvironmentName = Microsoft.Extensions.Hosting.Environments.Production;
    internal static readonly bool SerializeIndent = true;
#endif

	private static bool EnvironmentWantsIndents()
	{
		return HasVariableValue("API_SERIALIZE", "Indent");
	}

	private static bool HasVariableValue(string variableName, string mustEqual)
	{
		string? value = Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process);
		return value is not null
			   &&
			   StringComparer.CurrentCulture.Equals(mustEqual, value);
	}
}
