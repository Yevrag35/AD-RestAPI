namespace AD.Api.Validation;

/// <summary>
/// An interface that provides a mechanism to determine whether an implementation is empty.
/// </summary>
public interface ICanBeEmpty
{
	/// <summary>
	/// Indicates whether the object implementing this interface is classified as empty.
	/// </summary>
	bool IsEmpty { get; }
}