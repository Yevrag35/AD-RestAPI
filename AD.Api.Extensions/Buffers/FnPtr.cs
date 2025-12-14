namespace AD.Api.Buffers;

/// <summary>
/// Factory helpers for creating lightweight managed function pointer wrappers.
/// </summary>
/// <remarks>
/// Use the <c>Create</c> overloads to construct strongly-typed
/// wrappers around C# function pointers (<c>delegate*</c>). These wrappers provide an API-friendly <see cref="Invoke"/>
/// method and implicit conversions to simplify call sites while keeping the underlying call via an unmanaged-like pointer.
/// </remarks>
public static unsafe class FnPtr
{
	/// <summary>
	/// Creates a <see cref="FnPtr{T, TOut}"/> wrapper for the provided managed function pointer.
	/// </summary>
	/// <typeparam name="T">The parameter type accepted by the function pointer. Typically a <see langword="ref"/>-like type such as a ref struct.</typeparam>
	/// <typeparam name="TOut">The return type of the function pointer.</typeparam>
	/// <param name="ptr">A managed function pointer of shape <c>delegate* managed&lt;T, TOut&gt;</c>.</param>
	/// <returns>A <see cref="FnPtr{T, TOut}"/> that invokes the provided pointer.</returns>
	public static FnPtr<T, TOut> Create<T, TOut>(delegate* managed<T, TOut> ptr) where T : allows ref struct
	{
		return new FnPtr<T, TOut>(ptr);
	}

	/// <summary>
	/// Creates a <see cref="FnPtr{T1, T2, TOut}"/> wrapper for the provided managed function pointer with two input parameters.
	/// </summary>
	/// <typeparam name="T0">The first parameter type accepted by the function pointer.</typeparam>
	/// <typeparam name="T1">The second parameter type accepted by the function pointer.</typeparam>
	/// <typeparam name="TOut">The return type of the function pointer.</typeparam>
	/// <param name="ptr">A managed function pointer of shape <c>delegate* managed&lt;T1, T2, TOut&gt;</c>.</param>
	/// <returns>A <see cref="FnPtr{T1, T2, TOut}"/> that invokes the provided pointer.</returns>
	public static FnPtr<T0, T1, TOut> Create<T0, T1, TOut>(delegate* managed<T0, T1, TOut> ptr) where T0 : allows ref struct where T1 : allows ref struct
	{
		return new FnPtr<T0, T1, TOut>(ptr);
	}

	/// <summary>
	/// Throws an exception if the specified function pointer is <see langword="null"/>.
	/// </summary>
	/// <typeparam name="T">The type of the first parameter of the function pointer.</typeparam>
	/// <typeparam name="TOut">The return type of the function pointer.</typeparam>
	/// <param name="ptr">The function pointer to validate.</param>
	/// <param name="paramName">An optional parameter name of <paramref name="ptr"/> to pass to the exception.</param>
	/// <exception cref="ArgumentNullException"><paramref name="ptr"/> has a null function pointer.</exception>
	[StackTraceHidden]
	public static void ThrowIfNull<T, TOut>(FnPtr<T, TOut> ptr, [CallerArgumentExpression(nameof(ptr))] string? paramName = null) where T : allows ref struct
	{
		if (!ptr.IsValid)
		{
			throw new ArgumentNullException(paramName, "The function pointer cannot be null.");
		}
	}
	/// <summary>
	/// Throws an exception if the specified function pointer is <see langword="null"/>.
	/// </summary>
	/// <typeparam name="T">The type of the first parameter of the function pointer.</typeparam>
	/// <typeparam name="TOut">The return type of the function pointer.</typeparam>
	/// <param name="ptr">The function pointer to validate.</param>
	/// <param name="paramName">An optional parameter name of <paramref name="ptr"/> to pass to the exception.</param>
	/// <exception cref="ArgumentNullException"><paramref name="ptr"/> has a null function pointer.</exception>
	[StackTraceHidden]
	public static void ThrowIfNull<T0, T1, TOut>(FnPtr<T0, T1, TOut> ptr, [CallerArgumentExpression(nameof(ptr))] string? paramName = null) where T0 : allows ref struct where T1 : allows ref struct
	{
		if (!ptr.IsValid)
		{
			throw new ArgumentNullException(paramName, "The function pointer cannot be null.");
		}
	}
}

/// <summary>
/// A lightweight wrapper around a managed function pointer that accepts a single <typeparamref name="T"/> argument and returns <typeparamref name="TOut"/>.
/// </summary>
/// <typeparam name="T">The input parameter type. Constrained to <see langword="ref"/>-like types via <c>allows ref struct</c>.</typeparam>
/// <typeparam name="TOut">The return type of the pointer.</typeparam>
[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct FnPtr<T, TOut> where T : allows ref struct
{
	private readonly delegate* managed<T, TOut> _ptr;

	/// <summary>
	/// Gets a value indicating whether the current instance is valid.
	/// </summary>
	[MemberNotNullWhen(true, nameof(_ptr))]
	public bool IsValid => _ptr is not null;

	/// <summary>
	/// Initializes a new instance that wraps the specified function pointer.
	/// </summary>
	/// <param name="ptr">The managed function pointer to wrap.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="ptr"/> is null.</exception>
	public FnPtr(delegate* managed<T, TOut> ptr)
	{
		ArgumentNullException.ThrowIfNull(ptr);
		_ptr = ptr;
	}

	/// <summary>
	/// Invokes the underlying delegate with the specified argument and returns the result.
	/// </summary>
	/// <param name="arg">The input value to pass to the delegate. Cannot be null.</param>
	/// <returns>The result produced by invoking the delegate with the specified argument.</returns>
	public TOut Invoke(T arg)
	{
		return _ptr(arg);
	}

	/// <summary>
	/// Implicitly converts a raw managed function pointer to a <see cref="FnPtr{T, TOut}"/> wrapper.
	/// </summary>
	/// <param name="ptr">The managed function pointer to convert.</param>
	public static implicit operator FnPtr<T, TOut>(delegate* managed<T, TOut> ptr) => new FnPtr<T, TOut>(ptr);
}

/// <summary>
/// A lightweight wrapper around a managed function pointer that accepts two parameters and returns a result.
/// </summary>
/// <typeparam name="T0">The first input parameter type. Constrained to <see langword="ref"/>-like types via <c>allows ref struct</c>.</typeparam>
/// <typeparam name="T1">The second input parameter type. Constrained to <see langword="ref"/>-like types via <c>allows ref struct</c>.</typeparam>
/// <typeparam name="TOut">The return type of the pointer.</typeparam>
[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct FnPtr<T0, T1, TOut>
	where T0 : allows ref struct
	where T1 : allows ref struct
{
	private readonly delegate* managed<T0, T1, TOut> _ptr;

	/// <summary>
	/// Gets a value indicating whether the current instance is valid.
	/// </summary>
	public bool IsValid => _ptr is not null;
	/// <summary>
	/// Initializes a new instance that wraps the specified function pointer.
	/// </summary>
	/// <param name="ptr">The managed function pointer to wrap.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="ptr"/> is null.</exception>
	public FnPtr(delegate* managed<T0, T1, TOut> ptr)
	{
		ArgumentNullException.ThrowIfNull(ptr);
		_ptr = ptr;
	}
	/// <summary>
	/// Invokes the underlying function pointer with the given arguments.
	/// </summary>
	/// <param name="arg1">The first argument to pass to the function pointer.</param>
	/// <param name="arg2">The second argument to pass to the function pointer.</param>
	/// <returns>The value returned by the function pointer.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the underlying function pointer is null.</exception>*
	public TOut Invoke(T0 arg1, T1 arg2)
	{
		return _ptr(arg1, arg2);
	}

	/// <summary>
	/// Implicitly converts a raw managed function pointer to a <see cref="FnPtr{T1, T2, TOut}"/> wrapper.
	/// </summary>
	/// <param name="ptr">The managed function pointer to convert.</param>
	public static implicit operator FnPtr<T0, T1, TOut>(delegate* managed<T0, T1, TOut> ptr) => new FnPtr<T0, T1, TOut>(ptr);
}

/// <summary>
/// A lightweight wrapper around a managed function pointer that accepts two parameters and returns a result.
/// </summary>
/// <typeparam name="T0">The first input parameter type. Constrained to <see langword="ref"/>-like types via <c>allows ref struct</c>.</typeparam>
/// <typeparam name="T1">The second input parameter type. Constrained to <see langword="ref"/>-like types via <c>allows ref struct</c>.</typeparam>
/// <typeparam name="TOut">The return type of the pointer.</typeparam>
[StructLayout(LayoutKind.Sequential)]
public readonly unsafe struct FnPtr<T0, T1, T2, TOut>
	where T0 : allows ref struct
	where T1 : allows ref struct
	where T2 : allows ref struct
{
	private readonly delegate* managed<T0, T1, T2, TOut> _ptr;

	/// <summary>
	/// Gets a value indicating whether the current instance is valid.
	/// </summary>
	public bool IsValid => _ptr is not null;
	/// <summary>
	/// Initializes a new instance that wraps the specified function pointer.
	/// </summary>
	/// <param name="ptr">The managed function pointer to wrap.</param>
	/// <exception cref="ArgumentNullException">Thrown if <paramref name="ptr"/> is null.</exception>
	public FnPtr(delegate* managed<T0, T1, T2, TOut> ptr)
	{
		ArgumentNullException.ThrowIfNull(ptr);
		_ptr = ptr;
	}
	/// <summary>
	/// Invokes the underlying function pointer with the given arguments.
	/// </summary>
	/// <param name="arg1">The first argument to pass to the function pointer.</param>
	/// <param name="arg2">The second argument to pass to the function pointer.</param>
	/// <returns>The value returned by the function pointer.</returns>
	/// <exception cref="InvalidOperationException">Thrown if the underlying function pointer is null.</exception>*
	public TOut Invoke(T0 arg0, T1 arg1, T2 arg2)
	{
		return _ptr(arg0, arg1, arg2);
	}

	/// <summary>
	/// Implicitly converts a raw managed function pointer to a <see cref="FnPtr{T1, T2, TOut}"/> wrapper.
	/// </summary>
	/// <param name="ptr">The managed function pointer to convert.</param>
	public static implicit operator FnPtr<T0, T1, T2, TOut>(delegate* managed<T0, T1, T2, TOut> ptr) => new FnPtr<T0, T1, T2, TOut>(ptr);
}