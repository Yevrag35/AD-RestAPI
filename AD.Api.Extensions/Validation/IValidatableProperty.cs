using System.Linq.Expressions;

namespace AD.Api.Validation;

/// <summary>
/// Defines a contract for a property that can be validated and provides access to its value as an expression.
/// </summary>
/// <remarks>Implementations of this interface typically expose a property of type T that can be validated using
/// the returned expression. This interface is commonly used in validation frameworks to enable property-level
/// validation in a type-safe manner.</remarks>
/// <typeparam name="T">The type of the property value to be validated.</typeparam>
public interface IValidatableProperty<T>
{
	/// <summary>
	/// 
	/// </summary>
	/// <returns></returns>
	Expression<Func<object, T?>>? GetValidatableProperty();
}