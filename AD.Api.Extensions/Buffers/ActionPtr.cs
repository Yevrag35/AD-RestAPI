namespace AD.Api.Buffers;

/// <summary>
/// Represents a strongly-typed function pointer to an action that accepts two parameters of types <typeparamref
/// name="T0"/> and <typeparamref name="T1"/>.
/// </summary>
/// <remarks>This struct provides a type-safe wrapper around a native function pointer with two parameters,
/// enabling invocation of unmanaged code with ref struct arguments. It is intended for advanced scenarios where direct
/// function pointer invocation is required, such as interoperability or performance-critical code.</remarks>
/// <typeparam name="T0">The type of the first parameter passed to the function pointer. Must be a type that allows ref struct.</typeparam>
/// <typeparam name="T1">The type of the second parameter passed to the function pointer. Must be a type that allows ref struct.</typeparam>
[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct ActionPtr<T0, T1>
	where T0 : allows ref struct
	where T1 : allows ref struct
{
	private readonly delegate* managed<T0, T1, void> _ptr;

	/// <summary>
	/// Initializes a new instance of the ActionPtr class with the specified unmanaged function pointer.
	/// </summary>
	/// <remarks>Use this constructor to wrap a native function pointer for invocation from managed code. The
	/// generic parameters <typeparamref name="T0"/> and <typeparamref name="T1"/> specify the types of the arguments accepted by the function pointer.</remarks>
	/// <param name="ptr">The unmanaged function pointer to invoke. Must not be null.</param>
	/// <exception cref="ArgumentNullException"><paramref name="ptr"/> is null.</exception>
	public ActionPtr(delegate* managed<T0, T1, void> ptr)
	{
		ArgumentNullException.ThrowIfNull(ptr);
		_ptr = ptr;
	}

	public void Invoke(T0 arg0, T1 arg1)
	{
		_ptr(arg0, arg1);
	}
}