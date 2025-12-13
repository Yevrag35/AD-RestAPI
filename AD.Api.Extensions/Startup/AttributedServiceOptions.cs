using AD.Api.Assemblies;
using AD.Api.Startup.Services;
using AD.Api.Startup.Services.Internal;

namespace AD.Api.Startup;

/// <summary>
/// Options for configuring attributed dependency injection services.
/// </summary>
public sealed class AttributedServiceOptions
{
	private BindingFlags _dynamicMethodFlags;
	private Action<IAddServiceTypeExclusions>? _exclusionAction;
	private Action<Referencer>? _referencerAction;

	/// <summary>
	/// Gets or sets a value indicating whether duplicate service registrations are allowed.
	/// </summary>
	public bool AllowDuplicateServiceRegistrations { get; set; }

	/// <summary>
	/// Sets the assemblies to scan for attributed services.
	/// </summary>
	/// <remarks>
	/// If this is not set, the current application's <see cref="AppDomain.CurrentDomain"/> assemblies
	/// will be retrieved.
	/// </remarks>
	public Assembly[] AssembliesToScan { private get; set; }

	/// <summary>
	/// Sets the configuration for injecting into <see cref="DynamicServiceRegistrationMethodAttribute"/>
	/// decorated methods that accept a <see cref="IConfiguration"/> parameter.
	/// </summary>
	/// <remarks>
	/// It is best to provide this from the application startup, but it can be left <see langword="null"/> (unset) if 
	/// it's confirmed the application's dependency methods do not require it.
	/// <para>
	/// For this reason, <see langword="null"/>-checking is not performed when injecting into the dynamic methods.
	/// </para>
	/// </remarks>
	public IConfiguration? Configuration { internal get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to include assemblies flagged with <see cref="Assembly.IsDynamic"/> 
	/// in the scan.
	/// </summary>
	/// <remarks>
	/// This library has *NOT* been tested with dynamic assemblies, so set to <see langword="true"/> only if you know 
	/// what you are doing.
	/// </remarks>
	public bool IncludeDynamicAssembliesInScan { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to include public-visibility in the search for a 
	/// method decorated with <see cref="DynamicServiceRegistrationMethodAttribute"/>.
	/// </summary>
	/// <remarks>
	/// By default, only non-public methods are included in the search criteria. Regardless of this value, only
	/// <see langword="static"/> methods are included in the criteria.
	/// </remarks>
	public bool IncludePublicDynamicMethods
	{
		[DebuggerStepThrough]
		get => (_dynamicMethodFlags & BindingFlags.Public) != BindingFlags.Default;
		set
		{
			if (value)
			{
				_dynamicMethodFlags |= BindingFlags.Public;
			}
			else
			{
				_dynamicMethodFlags &= ~BindingFlags.Public;
			}
		}
	}

	/// <summary>
	/// Gets or sets a value indicating whether to include non-attributed assemblies in the scan.
	/// </summary>
	/// <remarks>
	/// By default, only assemblies with the <see cref="DependencyAssemblyAttribute"/> will be scanned.
	/// Setting this to <see langword="true"/> will remove this requirement however there will be a startup performance 
	/// penalty incurred.
	/// </remarks>
	public bool IncludeNonAttributedAssembliesInScan { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to ignore when multiple dynamic registration methods are 
	/// found in a single <see cref="DynamicServiceRegistrationAttribute"/> decorated object.
	/// </summary>
	/// <remarks>
	/// The default behavior is to throw an <see cref="AttributeDIStartupException"/> if multiple methods are found.
	/// When set to <see langword="true"/>, only the first method found (alphanumerically) will be invoked.
	/// </remarks>
	public bool IgnoreMultipleDynamicRegistrations { get; set; }

	/// <summary>
	/// Gets or sets a value indicating whether to throw an exception when a dynamic registration method is not found
	/// on a <see cref="DynamicServiceRegistrationAttribute"/> decorated object.
	/// </summary>
	/// <remarks>
	/// By default, missing methods are ignored. Setting this to <see langword="true"/> will throw an 
	/// <see cref="AttributeDIStartupException"/> instead.
	/// </remarks>
	public bool ThrowOnMissingDynamicRegistrationMethod { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="AttributedServiceOptions"/> class.
	/// </summary>
	internal AttributedServiceOptions()
	{
		_dynamicMethodFlags = BindingFlags.NonPublic | BindingFlags.Static;
		this.AssembliesToScan = [];
		this.Configuration = null;
	}

	/// <summary>
	/// Adds exclusions for service type registrations.
	/// </summary>
	/// <param name="configureExclusions">The action to configure exclusions.</param>
	/// <returns>The current <see cref="AttributedServiceOptions"/> for chaining.</returns>
	public AttributedServiceOptions AddExclusions(Action<IAddServiceTypeExclusions> configureExclusions)
	{
		_exclusionAction = configureExclusions;
		return this;
	}

	/// <summary>
	/// Loads references using the specified action.
	/// </summary>
	/// <param name="action">The action to load references.</param>
	/// <returns>The current <see cref="AttributedServiceOptions"/> for chaining.</returns>
	public AttributedServiceOptions LoadReferences(Action<Referencer> action)
	{
		ArgumentNullException.ThrowIfNull(action);
		_referencerAction = action;
		return this;
	}

	/// <summary>
	/// Gets the assemblies to be scanned based on the configuration.
	/// </summary>
	/// <returns>An enumerable of assemblies to be scanned.</returns>
	internal IEnumerable<Assembly> GetAssemblies()
	{
		if (_referencerAction is not null)
		{
			Referencer.LoadAll(_referencerAction);
		}

		Assembly[] allAssemblies = this.AssembliesToScan.Length > 0
			? this.AssembliesToScan
			: AppDomain.CurrentDomain.GetAssemblies();

		return !this.IncludeNonAttributedAssembliesInScan
			? allAssemblies.Where(this.IsAttributeServicableAssembly)
			: allAssemblies.Where(this.IsServicableAssembly);
	}

	internal BindingFlags GetDynamicMethodBindingFlags()
	{
		return _dynamicMethodFlags;
	}

	/// <summary>
	/// Gets the service type exclusions based on the configuration.
	/// </summary>
	/// <returns>The configured service type exclusions.</returns>
	internal IServiceTypeExclusions GetServiceTypeExclusions()
	{
		return ServiceTypeExclusions.ConfigureFromAction(_exclusionAction);
	}

	/// <summary>
	/// Determines whether the specified assembly is serviceable and contains the necessary attributes.
	/// </summary>
	/// <param name="assembly">The assembly to check.</param>
	/// <returns>True if the assembly is serviceable and contains the necessary attributes; otherwise, false.</returns>
	private bool IsAttributeServicableAssembly(Assembly assembly)
	{
		return this.IsServicableAssembly(assembly) && assembly.IsDefined(typeof(DependencyAssemblyAttribute), inherit: false);
	}

	/// <summary>
	/// Determines whether the specified assembly is serviceable.
	/// </summary>
	/// <param name="assembly">The assembly to check.</param>
	/// <returns>True if the assembly is serviceable; otherwise, false.</returns>
	private bool IsServicableAssembly(Assembly assembly)
	{
		return this.IncludeDynamicAssembliesInScan || !assembly.IsDynamic;
	}
}