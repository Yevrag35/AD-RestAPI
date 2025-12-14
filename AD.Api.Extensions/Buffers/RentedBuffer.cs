namespace AD.Api.Buffers;

/// <summary>
/// Provides methods for creating and managing rented buffers using the shared <see cref="ArrayPool{T}"/>.
/// </summary>
public static class RentedBuffer
{
	/// <summary>
	/// Creates a <see cref="RentedBuffer{T}"/> initialized with the specified values.
	/// </summary>
	/// <typeparam name="T">The type of elements in the buffer.</typeparam>
	/// <param name="values">A read-only span of values to initialize the buffer with.</param>
	/// <returns>A <see cref="RentedBuffer{T}"/> containing the specified values.</returns>
	public static RentedBuffer<T> Create<T>(params ReadOnlySpan<T> values)
	{
		return !values.IsEmpty
			? new(values, useEntireCapacity: false)
			: Empty<T>();
	}
	/// <summary>
	/// Returns an empty instance of <see cref="RentedBuffer{T}"/>.
	/// </summary>
	public static RentedBuffer<T> Empty<T>() => default;

	/// <summary>
	/// Rents a buffer with at least the specified minimum length.
	/// </summary>
	/// <typeparam name="T"><inheritdoc cref="RentedBuffer{T}" path="/typeparam[1]"/></typeparam>
	/// <param name="minimumLength">The minimum number of elements the buffer can hold.</param>
	/// <returns>A <see cref="RentedBuffer{T}"/> with at least the specified minimum capacity.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="minimumLength"/> is negative.</exception>
	[DebuggerStepThrough]
	public static RentedBuffer<T> RentMin<T>(int minimumLength)
	{
		return Rent<T>(minimumLength, useEntireCapacity: false);
	}
	public static RentedBuffer<T> RentSpan<T>(Span<T> span)
	{
		return new(span);
	}
	/// <summary>
	/// Rents a buffer with at least the specified minimum length and a flag indicating whether to use the entire capacity of the buffer.
	/// </summary>
	/// <typeparam name="T"><inheritdoc cref="RentedBuffer{T}" path="/typeparam[1]"/></typeparam>
	/// <param name="minimumLength">The minimum number of elements the buffer can hold.</param>
	/// <param name="useEntireCapacity">Indicates whether to make the entire capacity of the buffer available.</param>
	/// <returns>A <see cref="RentedBuffer{T}"/> with at least the specified minimum capacity.</returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="minimumLength"/> is negative.</exception>
	public static RentedBuffer<T> Rent<T>(int minimumLength, bool useEntireCapacity)
	{
		ArgumentOutOfRangeException.ThrowIfNegative(minimumLength);
		if (minimumLength == 0)
		{
			return Empty<T>();
		}

		return new(minimumLength: minimumLength, preValidated: true, useEntireCapacity: useEntireCapacity);
	}
	/// <summary>
	/// Rents a buffer with either the specified span or an <see cref="int"/> length that will be rented from <see cref="ArrayPool{T}"/>.
	/// </summary>
	/// <typeparam name="T">The type of elements in the buffer.</typeparam>
	/// <param name="either">A discriminated union of either a span or an integer length.</param>
	/// <returns>A <see cref="RentedBuffer{T}"/> with either a backing <see cref="Span{T}"/> or a rented array buffer, 
	/// depending on what was provided in <paramref name="either"/>.</returns>
	public static RentedBuffer<T> Rent<T>(RefEither<Span<T>, int> either)
	{
		return either.Index switch
		{
			1 => new(either.AsT1),
			2 when either.AsT2 > 0 => new(preValidated: true, minimumLength: either.AsT2, useEntireCapacity: true),
			_ => Empty<T>(),
		};
	}
}

/// <summary>
/// Represents a rented buffer of type <typeparamref name="T"/>, providing access to its underlying span and handling disposal to return it to the pool.
/// </summary>
/// <typeparam name="T">The type of elements in the buffer.</typeparam>
[StructLayout(LayoutKind.Auto)]
[DebuggerDisplay(@"\{{_span}\} IsRented = {_state.IsRented}, Length = {_length}")]
[CollectionBuilder(typeof(RentedBuffer), nameof(RentedBuffer.Create))]
public ref partial struct RentedBuffer<T> : IDisposable
{
	private T[]? _array;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	private int _length;
	private State _state;
	private Span<T> _span;

	/// <summary>
	/// Gets or sets the reference of the element at the specified index in the buffer.
	/// </summary>
	/// <remarks>
	/// The index must be within the bounds of the minimum length and not the capacity of the buffer.
	/// </remarks>
	/// <param name="index">The zero-based index of the element to get.</param>
	/// <returns>A reference to the element of type <typeparamref name="T"/> at the specified index in the buffer.</returns>
	/// <exception cref="IndexOutOfRangeException"/>
	public readonly ref T this[int index] => ref _span[index];

	/// <summary>
	/// Gets the span of elements in the rented buffer.
	/// </summary>
	public readonly Span<T> Span => _span;
	/// <summary>
	/// The number of elements that the buffer can store.
	/// </summary>
	public readonly int Capacity => _state.IsRented ? _array!.Length : _span.Length;
	/// <summary>
	/// Gets or sets whether the buffer should be cleared if the buffer has been rented from the <see cref="ArrayPool{T}"/>
	/// when this instance is disposed.
	/// </summary>
	public bool ClearOnDispose
	{
		readonly get => _state.ClearOnDispose;
		set => _state.ClearOnDispose = value;
	}
	/// <summary>
	/// Indicates whether this <see cref="RentedBuffer{T}"/> instance is empty or default-initialized.
	/// </summary>
	public readonly bool IsDefaultOrEmpty => _length == 0;
	/// <summary>
	/// Gets a value indicating whether the buffer is rented from the pool.
	/// </summary>
	[MemberNotNullWhen(true, nameof(_array))]
	public readonly bool IsRented => _state.IsRented;
	/// <summary>
	/// Gets the length of the specified minimum length.
	/// </summary>
	/// <remarks>
	/// This value will always be less than or equal to the capacity of the buffer.
	/// </remarks>
	public readonly int Length => _length;
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	public readonly int Count => _length;

	//readonly T IReadOnlyList<T>.this[int index] => this[index];

	/// <summary>
	/// Initializes a new instance of the <see cref="RentedBuffer{T}"/> with the specified minimum length.
	/// </summary>
	/// <param name="minimumLength">The minimum length of the buffer to rent.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="minimumLength"/> is negative.</exception>
	[DebuggerStepThrough]
	public RentedBuffer(int minimumLength)
		: this(minimumLength: minimumLength, preValidated: false, useEntireCapacity: false)
	{
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="RentedBuffer{T}"/> with the specified minimum length and a flag indicating whether to use the entire capacity of the buffer
	/// or just the minimum length.
	/// </summary>
	/// <param name="minimumLength">The minimum length of the buffer to rent.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="minimumLength"/> is negative.</exception>
	[DebuggerStepThrough]
	public RentedBuffer(int minimumLength, bool useEntireCapacity)
		: this(minimumLength: minimumLength, preValidated: false, useEntireCapacity: useEntireCapacity)
	{
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="RentedBuffer{T}"/> with the specified minimum length.
	/// </summary>
	/// <param name="minimumLength">The minimum length of the buffer to rent.</param>
	/// <param name="preValidated">Indicates that <paramref name="minimumLength"/> has been pre-guarded against being null or zero.</param>
	internal RentedBuffer(bool preValidated, int minimumLength, bool useEntireCapacity)
	{
		if (!preValidated)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(minimumLength);
			if (minimumLength == 0)
			{
				_state = new();
				_length = 0;
				_array = null;
				_span = [];
				return;
			}
		}

		T[] array = ArrayPool<T>.Shared.Rent(minimumLength);
		_state = new() { IsRented = true };
		int length = useEntireCapacity ? array.Length : minimumLength;
		_length = length;
		_array = array;
		_span = array.AsSpan(0, length);
	}
	/// <summary>
	/// Initializes a new instance of the RentedBuffer class using the specified span as the underlying buffer.
	/// </summary>
	/// <remarks>This constructor is intended for scenarios where a buffer is provided directly rather than rented
	/// from a pool. The buffer is not managed by the RentedBuffer instance and will not be disposed or returned when the
	/// instance is released.</remarks>
	/// <param name="nonRentedBuffer">A span of elements to use as the buffer. The buffer is not rented and will not be returned to a pool.</param>
	internal RentedBuffer(Span<T> nonRentedBuffer)
	{
		_state = new();
		_length = nonRentedBuffer.Length;
		_array = null;
		_span = nonRentedBuffer;
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="RentedBuffer{T}"/> with the specified values.
	/// </summary>
	/// <param name="values">The values to copy into the buffer.</param>
	[DebuggerStepThrough]
	internal RentedBuffer(scoped ReadOnlySpan<T> values, bool useEntireCapacity)
	{
		T[] array = ArrayPool<T>.Shared.Rent(values.Length);
		values.CopyTo(array);
		int length = useEntireCapacity ? array.Length : values.Length;
		_array = array;
		_span = array.AsSpan(0, length);
		_state = new() { IsRented = true };
		_length = length;
	}

	/// <summary>
	/// Disposes of the rented buffer, returning it to the pool if it was rented.
	/// </summary>
	public void Dispose()
	{
		T[]? array = _array;
		State state = _state;
		Debug.Assert(!_span.IsEmpty);
		this = default;

		if (state.IsRented)
		{
			ArrayPool<T>.Shared.Return(array!, state.ClearOnDispose || RuntimeHelpers.IsReferenceOrContainsReferences<T>());
		}
	}
	/// <summary>
	/// Returns an enumerator that iterates through the rented buffer.
	/// </summary>
	/// <returns>An enumerator for the buffer.</returns>
	[DebuggerStepThrough]
	public readonly Enumerator GetEnumerator()
	{
		return new Enumerator(_span);
	}

	/// <summary>
	/// Expands the buffer to at least the specified minimum capacity.
	/// </summary>
	/// <remarks>
	/// The contents of the buffer are preserved when resizing being copied to the new rented buffer. The old buffer is returned to the pool
	/// if it was rented.
	/// </remarks>
	/// <param name="newMinimumCapacity">The minimum capacity to expand the buffer to.</param>
	/// <returns>
	/// The capacity of the buffer after resizing, which may be greater than or equal to the specified <paramref name="newMinimumCapacity"/>.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="newMinimumCapacity"/> is less than the current <see cref="Length"/>.</exception>
	public int Resize(int newMinimumCapacity)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(newMinimumCapacity);

		int cap = this.Capacity;
		if ((uint)newMinimumCapacity <= (uint)cap && _state.IsRented) // If we are under the current capacity, we don't need to resize.
		{
			_length = cap;
			_span = _array;
			return cap;
		}

		T[] newArray = ArrayPool<T>.Shared.Rent(newMinimumCapacity);
		_span.CopyTo(newArray);

		if (_state.IsRented)
		{
			T[] oldArray = _array!;
			ArrayHelper.ReturnToPool(oldArray, _state.ClearOnDispose);
		}

		_state.IsRented = true;
		_array = newArray;
		_span = newArray;
		_length = newArray.Length;
		return newArray.Length;
	}

	/// <summary>
	/// Forms a slice out of the current buffer, beginning at the specified starting index extending to the end of the buffer.
	/// </summary>
	/// <param name="start">The zero-based starting index of the slice in the buffer.</param>
	/// <returns>
	/// A new span that is a slice of this buffer starting at the specified index and extending to the end of the buffer.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when the specified <paramref name="start"/> or end index is not in range (&lt;0 or &gt;Length).
	/// </exception>
	public readonly Span<T> Slice(int start)
	{
		return _span.Slice(start);
	}
	/// <summary>
	/// Forms a slice out of the current buffer, beginning at the specified starting index and of the specified length.
	/// </summary>
	/// <param name="start">The zero-based starting index of the slice in the buffer.</param>
	/// <param name="length">The desired length of the slice (exclusive).</param>
	/// <returns>
	/// A new span that is a slice of this buffer, starts at the specified index, and has the specified length.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException">
	/// Thrown when the specified <paramref name="start"/> or end index is not in range (&lt;0 or &gt;Length).
	/// </exception>
	public readonly Span<T> Slice(int start, int length)
	{
		return _span.Slice(start, length);
	}
	/// <summary>
	/// Forms a slice out of the backing array, beginning at the specified starting index extending to the end of the buffer.
	/// </summary>
	/// <param name="start"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentOutOfRangeException"/>
	/// <exception cref="InvalidOperationException">The current <see cref="RentedBuffer{T}"/> instance is not rented.</exception>
	public readonly Span<T> SliceBacking(int start)
	{
		ThrowIfNotRented(_state.IsRented);
		return _array.AsSpan(start);
	}
	/// <summary>
	/// Forms a slice out of the backing array, beginning at the specified starting index and of the specified length.
	/// </summary>
	/// <param name="start">
	/// The zero-based starting index of the slice in the buffer.
	/// </param>
	/// <param name="length">
	/// The desired length of the slice (exclusive).
	/// </param>
	/// <returns>
	/// A new span that is a slice of this buffer's backing , starts at the specified index, and has the specified length.
	/// </returns>
	/// <exception cref="ArgumentOutOfRangeException"/>
	/// <exception cref="InvalidOperationException">The current <see cref="RentedBuffer{T}"/> instance is not rented.</exception>
	public readonly Span<T> SliceBacking(int start, int length)
	{
		ThrowIfNotRented(_state.IsRented);
		return _array.AsSpan(start, length);
	}
	/// <summary>
	/// Returns a string representation of the rented buffer.
	/// </summary>
	/// <returns>A string that indicates whether the buffer is rented and its length.</returns>
	[DebuggerStepThrough]
	public override readonly string ToString()
	{
		return _length == 0
			? EMPTY_TOSTRING
			: $"{{IsRented = {_state.IsRented}, Length = {_length}, Capacity = {this.Capacity}}}";
	}

	/// <exception cref="InvalidOperationException"></exception>
	private static void ThrowIfNotRented([DoesNotReturnIf(false)] bool isRented)
	{
		if (!isRented)
		{
			throw new InvalidOperationException("Cannot access the backing array as this instance is not rented.");
		}
	}

	public readonly ReadOnlySpan<T> AsSpan()
	{
		return _span;
	}

	private const string EMPTY_TOSTRING = "{IsEmpty = true}";

	/// <summary>
	/// Enumerates the elements of a <see cref="RentedBuffer{T}"/>.
	/// </summary>
	[DebuggerStepThrough]
	[StructLayout(LayoutKind.Sequential)]
	public ref struct Enumerator
	{
		private readonly ReadOnlySpan<T> _span;
		private readonly int _length;
		private int _index;

		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		public readonly ref readonly T Current => ref _span[_index];

		/// <summary>
		/// Initializes the enumerator with the specified array.
		/// </summary>
		/// <param name="array">The array to enumerate.</param>
		internal Enumerator(ReadOnlySpan<T> array)
		{
			_span = array;
			_index = -1;
			_length = array.Length;
		}
		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		///		<see langword="true"/> if the enumerator was successfully advanced to the next element; 
		///		<see langword="false"/> if the enumerator has passed the end of the collection.
		/// </returns>
		public bool MoveNext()
		{
			int next = _index + 1;
			if ((uint)next >= (uint)_length)
			{
				_index = _length;
				return false;
			}

			_index = next;
			return true;
		}
		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		public void Reset()
		{
			_index = -1;
		}
	}
}