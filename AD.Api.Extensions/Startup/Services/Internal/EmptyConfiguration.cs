using Microsoft.Extensions.Primitives;

namespace AD.Api.Startup.Services.Internal;

internal sealed class EmptyConfiguration : IConfiguration, IConfigurationSection
{
	string? IConfiguration.this[string key]
	{
		get => null;
		set => throw new NotSupportedException();
	}

	string IConfigurationSection.Key => string.Empty;

	string IConfigurationSection.Path => string.Empty;

	string? IConfigurationSection.Value
	{
		get => null;
		set => throw new NotSupportedException();
	}

	IEnumerable<IConfigurationSection> IConfiguration.GetChildren()
	{
		return Array.Empty<IConfigurationSection>();
	}

	IChangeToken IConfiguration.GetReloadToken()
	{
		return null!;
	}

	IConfigurationSection IConfiguration.GetSection(string key)
	{
		return this;
	}
}
