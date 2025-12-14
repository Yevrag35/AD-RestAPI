using System.Globalization;
using System.Text;

namespace AD.Api.Validation;

/// <summary>
/// Provides helper methods for validating arguments and throwing exceptions in a consistent manner.
/// </summary>
/// <remarks>The methods in this class are intended to simplify and standardize argument validation logic
/// throughout an application. They throw well-formed exceptions when validation fails, making it easier to enforce
/// correct usage of APIs and to produce clear error messages. These methods are typically used at the start of public
/// or internal methods to check input parameters and ensure that preconditions are met.</remarks>
public static class ThrowHelper
{
	private static readonly CompositeFormat s_bufferToSmall = CompositeFormat.Parse("The buffer is too small to be used as a copy destination. Expected minimum length: {0}, Received length: {1}");

	/// <summary>
	/// Throws an <see cref="ArgumentException"/> with a message that indicates that the buffer is too small with the 
	/// optional parameter name and an format provider.
	/// </summary>
	/// <param name="actualValue">The actual length/size of the buffer which is less than the required minimum.</param>
	/// <param name="requiredMinimum">
	/// The minimum required length/size of the buffer that is greater than <paramref name="actualValue"/>.
	/// </param>
	/// <param name="parameterName">
	/// The parameter name of the buffer that is too small. If <see langword="null"/>, the parameter name is not included in the message.
	/// </param>
	/// <param name="provider">
	/// The format provider to use when formatting the message. 
	/// If <see langword="null"/>, the <see cref="CultureInfo.CurrentCulture"/> is used.
	/// </param>
	/// <exception cref="ArgumentException">The buffer is too small for the operation to be performed.</exception>
	[DoesNotReturn, StackTraceHidden]
	public static void BufferTooSmall(int requiredMinimum, int actualValue, [CallerArgumentExpression(nameof(actualValue))] string? parameterName = null, IFormatProvider? provider = null)
	{
		Debug.Assert(requiredMinimum > actualValue);

		string message = string.Format(
			provider: provider,
			format: s_bufferToSmall,
			arg0: requiredMinimum,
			arg1: actualValue);

		throw new ArgumentException(message, parameterName);
	}

	/// <summary>
	/// Throws an exception if the specified array is <see langword="null"/> or contains any null elements.
	/// </summary>
	/// <remarks>If the array's element type is a value type, this method does not check for null elements, as value
	/// types cannot be null. For reference type arrays, the method checks each element and throws if any are
	/// null.</remarks>
	/// <param name="array">The array to validate for null and for null elements.</param>
	/// <param name="paramName">The name of the parameter representing the array, used in exception messages. This value is optional.</param>
	/// <exception cref="ArgumentNullException"><paramref name="array"/> is null.</exception>
	/// <exception cref="ArgumentException">Thrown if the array contains a null element. The exception message includes the index of the first null element
	/// found.</exception>
	public static void ThrowIfAnyNull(
		[NotNullWhen(true)] Array? array,
		[CallerArgumentExpression(nameof(array))] string? paramName = null)
	{
		ArgumentNullException.ThrowIfNull(array, paramName);

		// If it's a value-type array, null elements are impossible.
		Type? elementType = array.GetType().GetElementType();
		if (elementType is null || elementType.IsValueType)
			return;

		int length = array.Length;
		if (length == 0)
			return;

		// ref to the first reference slot
		ref byte dataRef = ref MemoryMarshal.GetArrayDataReference(array);
		ref object? first = ref Unsafe.As<byte, object?>(ref dataRef);

		for (int i = 0; i < length; i++)
		{
			if (Unsafe.Add(ref first, i) is null)
			{
				ThrowArrayElementNull(paramName, i);
			}
		}

		static void ThrowArrayElementNull(string? name, int index)
			=> throw new ArgumentException(
				$"Array contains a null element at index -> {index}.",
				name);
	}

	private static readonly CompositeFormat s_outOfRange = CompositeFormat.Parse("{0} ('{1}') must not be negative but also not greater than '{2}'. (Parameter '{0}')\r\nActual value was {1}.");
	/// <summary>
	/// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified <paramref name="value"/> is negative  or
	/// greater than the specified <paramref name="other"/>.
	/// </summary>
	/// <remarks>This method is typically used to validate input parameters to ensure they fall within an acceptable
	/// range.</remarks>
	/// <param name="value">The integer value to validate. Must not be negative and must not exceed <paramref name="other"/>.</param>
	/// <param name="other">The upper limit, inclusive, that <paramref name="value"/> must not exceed. Must be less than or equal to <see
	/// cref="int.MaxValue"/>.</param>
	/// <param name="paramName">The name of the parameter being validated. This is automatically populated by the compiler if not explicitly
	/// provided.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is negative or greater than <paramref name="other"/>.</exception>
	[StackTraceHidden]
	public static void ThrowIfNegativeOrGreaterThan(int value, uint other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
	{
		Debug.Assert(other is <= int.MaxValue and not 0, "The other value should never be 0 and always less than or equal to int.MaxValue.");
		if ((uint)value > other)
		{
			throw new ArgumentOutOfRangeException(paramName, value,
				message: string.Format(
					CultureInfo.CurrentCulture,
					s_outOfRange,
					paramName,
					value,
					other));
		}

		//u ('4294967292') must be less than or equal to '4'. (Parameter 'u')
		// Actual value was 4294967292.
	}
	/// <summary>
	/// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified <paramref name="value"/> is negative  or
	/// greater than the specified <paramref name="other"/>.
	/// </summary>
	/// <remarks>This method is typically used to validate input parameters to ensure they fall within an acceptable
	/// range.</remarks>
	/// <param name="value">The integer value to validate. Must not be negative and must not exceed <paramref name="other"/>.</param>
	/// <param name="other">The upper limit, inclusive, that <paramref name="value"/> must not exceed. Must be less than or equal to <see
	/// cref="int.MaxValue"/>.</param>
	/// <param name="paramName">The name of the parameter being validated. This is automatically populated by the compiler if not explicitly
	/// provided.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is negative or greater than <paramref name="other"/>.</exception>
	[StackTraceHidden]
	public static void ThrowIfNegativeOrGreaterThan(long value, ulong other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
	{
		Debug.Assert(other is <= long.MaxValue and not 0, "The other value should never be 0 and always less than or equal to long.MaxValue.");
		if ((uint)value > other)
		{
			throw new ArgumentOutOfRangeException(paramName, value,
				message: string.Format(
					CultureInfo.CurrentCulture,
					s_outOfRange,
					paramName,
					value,
					other));
		}

		//u ('4294967292') must be less than or equal to '4'. (Parameter 'u')
		// Actual value was 4294967292.
	}
	/// <summary>
	/// Throws an <see cref="ArgumentOutOfRangeException"/> if the specified <paramref name="value"/> is negative  or
	/// greater than or equal to the specified <paramref name="other"/>.
	/// </summary>
	/// <remarks>This method is typically used to validate input parameters to ensure they fall within an acceptable
	/// range.</remarks>
	/// <param name="value">The integer value to validate.</param>
	/// <param name="other">The upper bound that <paramref name="value"/> must be less than. Must be less than or equal to <see
	/// cref="int.MaxValue"/>.</param>
	/// <param name="paramName">The name of the parameter being validated. This is automatically populated by the compiler when using  <see
	/// cref="CallerArgumentExpressionAttribute"/>.</param>
	/// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="value"/> is negative or greater than or equal to <paramref name="other"/>.</exception>
	[StackTraceHidden]
	public static void ThrowIfNegativeOrGreaterThanOrEqualTo(int value, uint other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
	{
		Debug.Assert(other is <= int.MaxValue and not 0, "The other value should never be 0 and always than or equal to int.MaxValue.");
		if ((uint)value >= other)
		{
			throw new ArgumentOutOfRangeException(paramName, value,
				message: string.Format(
					CultureInfo.CurrentCulture,
					"{0} ('{1}') must be an integer between 0 and '{2}'. (Parameter '{0}')\r\nActual value was {1}.",
					paramName,
					value,
					other));
		}
	}

	/// <summary>
	/// Casts the specified value to the given type parameter if possible, or throws an exception if the value is not of
	/// the expected type.
	/// </summary>
	/// <remarks>Use this method to ensure that an object is of a specific type at runtime. If the value is <see
	/// langword="null"/>, the method returns <see langword="null"/> without throwing an exception.</remarks>
	/// <typeparam name="T">The type to which the value is expected to be cast.</typeparam>
	/// <param name="value">The object to validate and cast to type <typeparamref name="T"/>. May be <see langword="null"/>.</param>
	/// <param name="paramName">The name of the parameter being validated. This is typically provided automatically by the compiler and is used in
	/// exception messages.</param>
	/// <returns>The value cast to type <typeparamref name="T"/>, or <see langword="null"/> if <paramref name="value"/> is <see
	/// langword="null"/>.</returns>
	/// <exception cref="ArgumentException">Thrown if <paramref name="value"/> is not <see langword="null"/> and is not of type <typeparamref name="T"/>.</exception>
	[return: NotNullIfNotNull(nameof(value))]
	public static T? ThrowIfNot<T>(object? value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
	{
		if (value is null)
			return default;

		if (value is not T typed)
		{
			throw new ArgumentException(
				message: string.Format(
					CultureInfo.CurrentCulture,
					"Value of parameter '{0}' is of type '{1}' but must be of type '{2}'.",
					paramName,
					value.GetType().FullName,
					typeof(T).FullName),
				paramName);
		}

		return typed;
	}
}