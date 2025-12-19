using AD.Api.Buffers;
using AD.Api.Reflection.Exceptions;
using AD.Api.Startup.Services;

namespace AD.Api.Attributes.Services;

/// <summary>
/// A base attribute for defining services that will be automatically added to the dependency injection container
/// including the service type, implementation type, and lifetime.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
public abstract class AddToDepedencyInjectionAttribute : AutomaticDependencyInjectionAttribute
{
	/// <summary>
	/// The <see cref="Type"/> implementing the service.
	/// </summary>
	/// <remarks>
	/// Will default to the decorated type if not set or <see langword="null"/>.
	/// </remarks>
	protected Type? Implementation { get; set; }

	/// <inheritdoc cref="ServiceLifetime"/>
	public required ServiceLifetime Lifetime { get; init; }
	/// <summary>
	/// The <see cref="Type"/> of the service.
	/// </summary>
	/// <remarks>
	/// Will default to the decorated type if not set or <see langword="null"/>.
	/// </remarks>
	protected Type? Service { get; set; }

	/// <summary>
	/// Constructs and enumerates all <see cref="ServiceDescriptor"/> objects from the specified type.
	/// </summary>
	/// <returns>
	/// A <see cref="List{T}"/> of <see cref="ServiceDescriptor"/> objects that were created from the 
	/// specified type.
	/// </returns>
	/// <exception cref="ArgumentException"/>
	/// <inheritdoc 
	///     cref="TryCreateDescriptorFromAttribute(ServiceRegistrationBaseAttribute, Type, in IServiceTypeExclusions, out ServiceDescriptor)"
	///     path="/exception"/>
	[DebuggerStepThrough]
	internal static ArraySlice<ServiceDescriptor> CreateDescriptorsFromType(
		Type type,
		IServiceTypeExclusions exclusions)
	{
		object[] atts = type.GetCustomAttributes(
			typeof(AddToDepedencyInjectionAttribute),
			inherit: false);

		if (atts.Length == 0)
			return [];

		ServiceDescriptor[] descriptors = new ServiceDescriptor[atts.Length];
		int count = 0;
		foreach (AddToDepedencyInjectionAttribute attribute in atts)
		{
			if (TryCreateDescriptorFromAttribute(attribute, type, exclusions, out ServiceDescriptor? descriptor))
			{
				descriptors[count++] = descriptor;
			}
		}

		return new(descriptors, count);
	}

	[DebuggerStepThrough]
	private static bool AnyGenericInterfaceMatches(
		Type serviceType,
		[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
		Type implementationType)
	{
		Type[] intTypes = implementationType.GetInterfaces();
		for (int i = 0; i < intTypes.Length; i++)
		{
			Type interfaceType = intTypes[i];

			if (!interfaceType.IsGenericTypeDefinition && interfaceType.IsGenericType)
			{
				interfaceType = interfaceType.GetGenericTypeDefinition();
			}

			if (interfaceType.IsGenericTypeDefinition
				&&
				interfaceType.Equals(serviceType))
			{
				return true;
			}
		}

		return false;
	}

	[DebuggerStepThrough]
	private static bool ImplementsType(Type serviceType, Type implementationType)
	{
		if (serviceType.IsAssignableFrom(implementationType))
		{
			return true;
		}
		else if (serviceType.IsGenericTypeDefinition)
		{
			if (!implementationType.IsGenericTypeDefinition)
			{
				implementationType = implementationType.GetGenericTypeDefinition();
			}

			if (serviceType.IsInterface)
			{
				return AnyGenericInterfaceMatches(serviceType, implementationType);
			}
			else
			{
				return implementationType.IsSubclassOf(serviceType);
			}
		}

		return false;
	}

	/// <inheritdoc cref="Type.GetGenericTypeDefinition" path="/exception"/>
	/// <inheritdoc cref="ValidateImplementationType(Type, Type)" path="/exception"/>
	private static bool TryCreateDescriptorFromAttribute(
		AddToDepedencyInjectionAttribute attribute,
		Type type,
		IServiceTypeExclusions exclusions,
		[NotNullWhen(true)] out ServiceDescriptor? descriptor)
	{
		attribute.Service ??= type;
		attribute.Implementation ??= type;

		if (exclusions.IsExcluded(attribute.Service))
		{
			descriptor = null;
			return false;
		}

		if (attribute.Service.IsGenericTypeDefinition && !attribute.Implementation.IsGenericTypeDefinition)
		{
			attribute.Implementation = attribute.Implementation.GetGenericTypeDefinition();
		}

		ValidateImplementationType(attribute.Service, attribute.Implementation);
		descriptor = new(attribute.Service, attribute.Implementation, attribute.Lifetime);
		return true;
	}

	/// <exception cref="MissingConstructorException"></exception>
	/// <exception cref="TypeNotAssignableException"></exception>
	[DebuggerStepThrough]
	protected static void ValidateImplementationType(Type serviceType, Type implementationType)
	{
		if (!ReferenceEquals(serviceType, implementationType) && !ImplementsType(serviceType, implementationType))
		{
			throw new TypeNotAssignableException(serviceType, implementationType);
		}
	}
}

