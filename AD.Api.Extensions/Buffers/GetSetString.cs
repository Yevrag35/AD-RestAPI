namespace AD.Api.Buffers;

/// <summary>
/// Provides a type-safe wrapper for unmanaged getter and setter function pointers that operate on a reference type and
/// a string value.
/// </summary>
/// <remarks>This struct enables direct invocation of unmanaged getter and setter delegates for a string property
/// on instances of the specified reference type. It is intended for advanced scenarios where performance and
/// interoperability with unmanaged code are required. The struct is unsafe and should be used with care, as incorrect
/// usage may result in undefined behavior or application instability.</remarks>
/// <typeparam name="TClass">The reference type on which the getter and setter function pointers operate.</typeparam>
[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct GetSetString<TClass> where TClass : class
{
	private readonly FnPtr<TClass, string?> _getter;
	private readonly ActionPtr<TClass, string?> _setter;

	/// <summary>
	/// Initializes a new instance of the GetSetPointer class using the specified getter and setter delegates for string
	/// properties on the target class.
	/// </summary>
	/// <remarks>Both delegates must be compatible with unmanaged function pointers and should not capture any
	/// managed references. This constructor is intended for advanced scenarios where direct property access via delegates
	/// is required, such as interop or performance-critical code.</remarks>
	/// <param name="getter">A pointer to a delegate that retrieves the string value from an instance of <typeparamref name="TClass"/>. The
	/// delegate should accept an instance of <typeparamref name="TClass"/> and return a string or null.</param>
	/// <param name="setter">A pointer to a delegate that sets the string value on an instance of <typeparamref name="TClass"/>. The delegate
	/// should accept an instance of <typeparamref name="TClass"/> and a string value to assign.</param>
	public GetSetString(delegate* managed<TClass, string?> getter, delegate* managed<TClass, string?, void> setter)
	{
		_getter = new(getter);
		_setter = new(setter);
	}

	/// <summary>
	/// Retrieves a string value from the specified model instance.
	/// </summary>
	/// <param name="model">The model instance from which to extract the value. Cannot be null.</param>
	/// <returns>A string value obtained from the model, or null if no value is available.</returns>
	public string? GetValue([DisallowNull] TClass model)
	{
		return _getter.Invoke(model);
	}
	/// <summary>
	/// Sets the specified value on the provided model instance.
	/// </summary>
	/// <param name="model">The model instance on which to set the value. Cannot be null.</param>
	/// <param name="value">The value to assign to the model. May be null depending on the model's requirements.</param>
	public void SetValue([DisallowNull] TClass model, string? value)
	{
		_setter.Invoke(model, value);
	}
}
