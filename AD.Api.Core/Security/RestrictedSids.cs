using AD.Api.Attributes.Services;
using System.Collections.Frozen;
using System.ComponentModel;

namespace AD.Api.Core.Security;

/// <summary>
/// Defines a method to check if a security identifier (SID) is restricted.
/// </summary>
public interface IRestrictedSids
{
	/// <summary>
	/// Determines whether the specified security identifier is restricted.
	/// </summary>
	/// <param name="securityIdentifier">The security identifier to check.</param>
	/// <returns><see langword="true"/> if the SID is restricted; otherwise, <see langword="false"/>.</returns>
	bool Contains(string securityIdentifier);
}

/// <summary>
/// Provides a base class for handling restricted SIDs.
/// </summary>
[DynamicDependencyRegistration]
internal abstract class RestrictedSids : IRestrictedSids
{
	/// <summary>
	/// Gets a value indicating whether the restricted SIDs set is empty.
	/// </summary>
	internal abstract bool IsEmpty { get; }

	/// <summary>
	/// Determines whether the specified security identifier is restricted.
	/// </summary>
	/// <param name="securityIdentifier">The security identifier to check.</param>
	/// <returns><see langword="true"/> if the SID is restricted; otherwise, <see langword="false"/>.</returns>
	public abstract bool Contains(string securityIdentifier);

	/// <summary>
	/// Adds the <see cref="RestrictedSids"/> services to the service collection based on configuration settings.
	/// </summary>
	/// <param name="services">The service collection to add services to.</param>
	/// <param name="configuration">The configuration used to retrieve restricted SIDs settings.</param>
	[DynamicDependencyRegistrationMethod]
	[EditorBrowsable(EditorBrowsableState.Never)]
	private static void AddToServices(IServiceCollection services, IConfiguration configuration)
	{
		IConfigurationSection section = configuration
			.GetRequiredSection("Settings")
			.GetSection("Restrictions")
			.GetSection("RestrictedSIDs");

		string[]? set = section.Get<string[]>();

		_ = set is null || set.Length <= 0
			? services.AddSingleton<RestrictedSids, NoRestrictedSids>()
			: services.AddSingleton<RestrictedSids>(new HasRestrictedSids(set));

		if (!OperatingSystem.IsWindows())
		{
			_ = services.AddSingleton<IRestrictedSids>(x => x.GetRequiredService<RestrictedSids>());
		}
	}

	/// <summary>
	/// Represents a set of restricted SIDs.
	/// </summary>
	private sealed class HasRestrictedSids : RestrictedSids
	{
		private readonly FrozenSet<string> _sids;

		/// <summary>
		/// Gets a value indicating whether the restricted SIDs set is empty.
		/// </summary>
		internal override bool IsEmpty => false;

		/// <summary>
		/// Initializes a new instance of the <see cref="HasRestrictedSids"/> class with the specified SIDs.
		/// </summary>
		/// <param name="sids">An array of restricted SIDs.</param>
		internal HasRestrictedSids(string[] sids)
		{
			_sids = FrozenSet.ToFrozenSet(sids, StringComparer.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Determines whether the specified security identifier is restricted.
		/// </summary>
		/// <param name="securityIdentifier">The security identifier to check.</param>
		/// <returns><see langword="true"/> if the SID is restricted; otherwise, <see langword="false"/>.</returns>
		public override bool Contains(string securityIdentifier)
		{
			return _sids.Contains(securityIdentifier);
		}
	}

	/// <summary>
	/// Represents an empty set of restricted SIDs.
	/// </summary>
	private sealed class NoRestrictedSids : RestrictedSids
	{
		/// <summary>
		/// Gets a value indicating whether the restricted SIDs set is empty.
		/// </summary>
		internal override bool IsEmpty => true;

		/// <summary>
		/// Determines whether the specified security identifier is restricted.
		/// </summary>
		/// <param name="securityIdentifier">The security identifier to check.</param>
		/// <returns><see langword="false"/> indicating the SID is not restricted.</returns>
		public override bool Contains(string securityIdentifier)
		{
			return false;
		}
	}
}
