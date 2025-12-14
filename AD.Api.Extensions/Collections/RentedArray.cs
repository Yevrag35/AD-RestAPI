using AD.Api.Buffers;

namespace AD.Api.Collections;

/// <summary>
/// Provides methods for creating and managing rented arrays, which are temporary arrays designed for efficient memory usage.
/// </summary>
/// <remarks>
/// The <see cref="RentedArray"/> class offers functionality to create rented arrays with specified values or to retrieve an empty rented array.
/// Rented arrays are typically used to minimize memory allocations in performance-critical scenarios.
/// </remarks>
[DebuggerDisplay("Length = {Length}")]
public abstract class RentedArray : IDisposable
{
	/// <summary>
	/// Gets the total number of elements that the internal array can hold without resizing.
	/// </summary>
	public abstract int Capacity { get; }
	/// <summary>
	/// Gets a value indicating whether the array has been disposed.
	/// </summary>
	private protected abstract bool Disposed { get; }
	/// <summary>
	/// Gets the intended length of the rented array.
	/// </summary>
	public abstract int Length { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="RentedArray"/> class.
	/// </summary>
	private protected RentedArray() { }

	/// <summary>
	/// Retrieves the value at the specified index.
	/// </summary>
	/// <param name="index">The zero-based index of the value to retrieve. Must be within the valid range of indices.</param>
	/// <returns>The value at the specified index, or <see langword="null"/> if the value is not set or the index is invalid.</returns>
	/// <exception cref="ArgumentOutOfRangeException"/>
	public abstract object? GetValue(int index);

	/// <summary>
	/// Creates a new <see cref="RentedArray{T}"/> initialized with the specified values.
	/// </summary>
	/// <typeparam name="T">The type of elements in the array.</typeparam>
	/// <param name="values">A read-only span containing the values to initialize the rented array.</param>
	/// <returns>A <see cref="RentedArray{T}"/> containing the specified values, or an empty array if <paramref name="values"/> is empty.</returns>
	public static RentedArray<T> Create<T>(params ReadOnlySpan<T> values)
	{
		return !values.IsEmpty ? new(values) : Empty<T>();
	}

	/// <summary>
	/// Returns a shared empty <see cref="RentedArray{T}"/> instance.
	/// </summary>
	/// <typeparam name="T">The type of elements in the array.</typeparam>
	/// <returns>An empty <see cref="RentedArray{T}"/>.</returns>
	public static RentedArray<T> Empty<T>() => EmptyArray<T>.Value;

	/// <summary>
	/// Determines whether the specified <see cref="RentedArray"/> is null or empty.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <param name="array">The <see cref="RentedArray"/> to check.</param>
	/// <returns><see langword="true"/> if the <paramref name="array"/> is <see langword="null"/>, disposed, or has a length of 0; otherwise, <see
	/// langword="false"/>.</returns>
	public static bool IsNullOrEmpty([NotNullWhen(false)] RentedArray? array)
	{
		return array is null or { Disposed: true } or { Length: 0 };
	}

	/// <summary>
	/// Releases the resources used by the <see cref="RentedArray"/> and returns the array to the pool.
	/// </summary>
	public void Dispose()
	{
		this.Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	/// <summary>
	/// Releases the resources used by the <see cref="RentedArray"/> and returns the array to the pool.
	/// </summary>
	/// <param name="disposing">Specifies whether to release both managed and unmanaged resources (<see langword="true"/>) or only unmanaged resources (<see langword="false"/>).</param>
	protected abstract void Dispose(bool disposing);

	/// <summary>
	/// Provides a cached empty <see cref="RentedArray{T}"/> instance for each type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type of elements in the array.</typeparam>
	private static class EmptyArray<T>
	{
		/// <summary>
		/// The cached empty <see cref="RentedArray{T}"/> instance.
		/// </summary>
		internal static readonly RentedArray<T> Value = new();
	}
}

/// <summary>
/// Represents a rented array of elements that is pooled to minimize memory allocations.
/// </summary>
/// <remarks>
/// The <see cref="RentedArray{T}"/> class provides a managed wrapper around arrays rented from the <see cref="ArrayPool{T}"/>.
/// It is designed to reduce memory allocations by reusing arrays from the pool. The array is automatically returned to the pool when the instance is disposed.
/// The length of the rented array is specified at construction, and the array can be accessed and modified using the indexer or by obtaining a <see cref="Span{T}"/> via the <see cref="AsSpan"/> method.
/// The <see cref="ClearOnDispose"/> property determines whether the array is cleared before being returned to the pool. It is important to call <see cref="Dispose"/> when the instance is no longer needed to ensure the array is returned to the pool.
/// Failure to do so may result in memory leaks.
/// </remarks>
/// <typeparam name="T">The type of elements stored in the array.</typeparam>
[CollectionBuilder(typeof(RentedArray), nameof(Create))]
public sealed class RentedArray<T> : RentedArray, IReadOnlyArray<T>
{
	private bool _disposed;
	private T[] _array;
	private int _length;
	private int _version;

	/// <summary>
	/// Gets or sets the element at the specified index in the collection.
	/// </summary>
	/// <param name="index">The zero-based index of the element to get or set. Must be within the bounds of the collection.</param>
	/// <returns>The element at the specified index.</returns>
	/// <exception cref="IndexOutOfRangeException"/>
	public T this[int index]
	{
		get => _array[index];
		set => _array[index] = value;
	}

	/// <inheritdoc/>
	public override int Capacity => _array.Length;

	/// <summary>
	/// Gets or sets a value indicating whether the internal array should be cleared when this object is disposed.
	/// </summary>
	public bool ClearOnDispose { get; set; }
	/// <inheritdoc/>
	private protected override bool Disposed => _disposed;
	/// <inheritdoc/>
	public override int Length => _length;

	/// <inheritdoc/>
	[DebuggerBrowsable(DebuggerBrowsableState.Never)]
	int IReadOnlyCollection<T>.Count => this.Length;

	/// <summary>
	/// Initializes a new instance of the <see cref="RentedArray{T}"/> class with a specified minimum length.
	/// </summary>
	/// <remarks>
	/// If <paramref name="minimumLength"/> is 0, an empty array is used. Otherwise, an array is rented from the shared <see cref="ArrayPool{T}"/>.
	/// The rented array will have a length greater than or equal to <paramref name="minimumLength"/>.
	/// </remarks>
	/// <param name="minimumLength">The minimum number of elements the rented array should be able to hold. Must be greater than or equal to 0.</param>
	public RentedArray(int minimumLength)
	{
		minimumLength = Math.Max(minimumLength, 0);
		T[] array = minimumLength != 0
			? ArrayPool<T>.Shared.Rent(minimumLength)
			: [];

		_array = array;
		_length = minimumLength;
		_disposed = array.Length == 0;
	}
	/// <summary>
	/// Initializes a new instance of the <see cref="RentedArray"/> class.
	/// </summary>
	/// <remarks>This constructor is intended for internal use only and initializes the array with default values.</remarks>
	internal RentedArray()
	{
		_array = [];
		_length = 0;
		_disposed = true;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="RentedArray{T}"/> class with the specified values.
	/// </summary>
	/// <remarks>
	/// The rented array is initialized by copying the elements from the provided <paramref name="values"/> span.
	/// The caller is responsible for ensuring that the span contains the desired data to populate the array.
	/// </remarks>
	/// <param name="values">A read-only span containing the values to initialize the rented array. The length of the span determines the size of the rented array.</param>
	internal RentedArray(ReadOnlySpan<T> values)
		: this(values.Length)
	{
		values.CopyTo(_array);
	}

	/// <summary>
	/// Returns a <see cref="Span{T}"/> representing the intended portion of the underlying array with <see cref="Length"/> elements.
	/// </summary>
	/// <remarks>
	/// The returned <see cref="Span{T}"/> will have a length equal to <see cref="Length"/>.
	/// If the array is empty, an empty <see cref="Span{T}"/> is returned.
	/// </remarks>
	/// <returns>A <see cref="Span{T}"/> containing the elements of the underlying array, or an empty <see cref="Span{T}"/> if this array is empty.</returns>
	public Span<T> AsSpan()
	{
		return _length > 0
			? _array.AsSpan(0, _length)
			: [];
	}

	/// <inheritdoc/>
	[DebuggerStepThrough]
	ReadOnlySpan<T> IReadOnlyArray<T>.AsSpan()
	{
		return this.AsSpan();
	}
	/// <summary>
	/// Returns a <see cref="Span{T}"/> that represents a segment of the underlying array, starting at the specified index
	/// and spanning the specified number of elements.
	/// </summary>
	/// <param name="start">The zero-based index at which the span begins. Must be non-negative and less than the length of the array.</param>
	/// <param name="length">The number of elements in the span. Must be non-negative and the sum of <paramref name="start"/> and <paramref
	/// name="length"/> must not exceed the length of the array.</param>
	/// <returns>A <see cref="Span{T}"/> representing the specified segment of the array. If the capacity of the underlying array is
	/// zero, returns an empty span.</returns>
	public Span<T> AsSpan(int start, int length)
	{
		return _length > 0
			? _array.AsSpan(start, length)
			: [];
	}
	/// <summary>
	/// Clears all elements in the collection, setting them to their default values.
	/// </summary>
	/// <remarks>This method resets all elements in the underlying array to their default values  (e.g., <see
	/// langword="null"/> for reference types, <see langword="false"/> for <see langword="bool"/>,  and <see langword="0"/>
	/// for numeric types). The size of the collection remains unchanged.</remarks>
	public void Clear()
	{
		Array.Clear(_array, 0, _array.Length);
	}
	/// <summary>
	/// Copies the elements of the specified collection into the current instance, starting at the first index.
	/// </summary>
	/// <remarks>The method copies all elements from the specified <paramref name="collection"/> into the internal
	/// array of the current instance, starting at index 0. Ensure that the current instance is not disposed before calling
	/// this method.</remarks>
	/// <param name="collection">The collection whose elements are to be copied. Must not be null.</param>
	/// <exception cref="ObjectDisposedException"/>
	public void CopyFrom(ICollection collection)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		collection.CopyTo(_array, 0);
	}

	/// <summary>
	/// Releases the resources used by the <see cref="RentedArray{T}"/> optionally disposing managed resources.
	/// </summary>
	/// <param name="disposing">
	/// <see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.
	/// </param>
	protected override void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			_disposed = true;
			_version++;
			T[]? array = _array;
			_array = [];
			_length = 0;

			if (disposing)
			{
				_ = ArrayHelper.ReturnToPool(array, this.ClearOnDispose);
			}
		}
	}

	/// <summary>
	/// Returns an enumerator that iterates through the collection.
	/// </summary>
	/// <returns>An enumerator that can be used to iterate through the collection.</returns>
	/// <exception cref="ObjectDisposedException">Thrown if the <see cref="RentedArray{T}"/> has already been disposed.</exception>
	public Enumerator GetEnumerator()
	{
		return new(this);
	}

	/// <inheritdoc/>
	[DebuggerStepThrough]
	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	/// <inheritdoc/>
	[DebuggerStepThrough]
	IEnumerator IEnumerable.GetEnumerator()
	{
		return this.GetEnumerator();
	}

	/// <inheritdoc/>
	[DebuggerStepThrough]
	public override object? GetValue(int index)
	{
		return _array[index];
	}

	/// <summary>
	/// Resizes the internal array to accommodate the specified size, ensuring it meets the required capacity.
	/// </summary>
	/// <remarks>If <paramref name="newSize"/> is less than or equal to the current capacity, the internal array is
	/// truncated to the specified size. If <paramref name="newSize"/> exceeds the current capacity, the internal array is
	/// resized to the next power of two that can accommodate the requested size, with additional adjustments to minimize
	/// memory overhead.  The method uses an <see cref="ArrayPool{T}"/> to manage memory efficiently, returning the old
	/// array to the pool after resizing. If <see cref="ClearOnDispose"/> is set to <see langword="true"/>, the old array
	/// is cleared before being returned to the pool.</remarks>
	/// <param name="newSize">The desired new size of the array. Must be greater than or equal to the current length.</param>
	/// <returns>The new size of the internal array after resizing.</returns>
	/// <exception cref="ObjectDisposedException">Thrown if the <see cref="RentedArray{T}"/> has already been disposed.</exception>
	public int Resize(int newSize)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		ArgumentOutOfRangeException.ThrowIfLessThan(newSize, _length);
		_version++;
		if (newSize <= _array.Length)
		{
			_length = newSize;
			return newSize;
		}

		int old = newSize;
		newSize = BitHelper.RoundUpToPowerOf2Unsafe(newSize);
		if (newSize - old < old / 2)
		{
			newSize = BitHelper.RoundUpToPowerOf2Unsafe(newSize);
		}

		T[] newArray = ArrayPool<T>.Shared.Rent(newSize);
		T[] oldArray = _array;
		Array.Copy(oldArray, newArray, _length);

		_array = newArray;
		_length = newArray.Length;
		bool returned = ArrayHelper.ReturnToPool(oldArray, this.ClearOnDispose);
		Debug.Assert(returned, "The old array should have been returned to the pool.");

		return _length;
	}

	/// <summary>
	/// Sorts the elements in the collection in ascending order using the default comparer.
	/// </summary>
	/// <exception cref="ObjectDisposedException">The <see cref="RentedArray{T}"/> has already been disposed.</exception>
	public void Sort()
	{
		this.Sort(comparer: null);
	}
	/// <summary>
	/// Sorts the elements in the collection using the specified comparer.
	/// </summary>
	/// <remarks>If <paramref name="comparer"/> is <see langword="null"/>, the default comparer for the type
	/// <typeparamref name="T"/> is used.  The sort operation is performed in-place and modifies the current
	/// collection.</remarks>
	/// <param name="comparer">An <see cref="IComparer{T}"/> implementation to use for comparing elements, or <see langword="null"/> to use the
	/// default comparer for the type <typeparamref name="T"/>.</param>
	/// <exception cref="ObjectDisposedException">The <see cref="RentedArray{T}"/> has already been disposed.</exception>
	public void Sort(IComparer<T>? comparer)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		_version++;
		Array.Sort(_array, 0, _length, comparer);
	}
	/// <summary>
	/// Sorts a range of elements in the collection using the specified comparer.
	/// </summary>
	/// <remarks>This method sorts the specified range of elements in place. If <paramref name="comparer"/> is <see
	/// langword="null"/>, the default comparer for the element type is used. The sort operation is stable, meaning that
	/// the relative order of equal elements is preserved.</remarks>
	/// <param name="index">The zero-based starting index of the range to sort. Must be non-negative and less than the length of the
	/// collection.</param>
	/// <param name="length">The number of elements in the range to sort. Must be non-negative and the range defined by <paramref name="index"/>
	/// and <paramref name="length"/> must not exceed the bounds of the collection.</param>
	/// <param name="comparer">The comparer to use for comparing elements, or <see langword="null"/> to use the default comparer for the element
	/// type.</param>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative or greater than or equal to the length of the collection, or <paramref name="length"/> is negative or exceeds the bounds of the collection.</exception>
	/// <exception cref="ObjectDisposedException">The <see cref="RentedArray{T}"/> has already been disposed.</exception>
	public void Sort(int index, int length, IComparer<T>? comparer)
	{
		ObjectDisposedException.ThrowIf(_disposed, this);
		_version++;
		ArgumentOutOfRangeException.ThrowIfNegative(index);
		ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _length);
		ArgumentOutOfRangeException.ThrowIfGreaterThan((uint)length, (uint)_length - (uint)index, nameof(length));
		Array.Sort(_array, index, length, comparer);
	}

	/// <summary>
	/// Enumerates the elements of a <see cref="RentedArray{T}"/>.
	/// </summary>
	[StructLayout(LayoutKind.Auto)]
	public struct Enumerator : IEnumerator<T>
	{
		private T _current;
		private RentedArray<T> _array;
		private int _index;
		private int _version;

		/// <summary>
		/// Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		public readonly T Current => _current;
		/// <inheritdoc/>
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly object? IEnumerator.Current => this.Current;

		/// <summary>
		/// Initializes a new instance of the <see cref="Enumerator"/> struct for the specified <see cref="RentedArray{T}"/>.
		/// </summary>
		/// <param name="array">The <see cref="RentedArray{T}"/> to enumerate.</param>
		/// <param name="length">The number of elements to enumerate.</param>
		/// <exception cref="ObjectDisposedException">Thrown if the <paramref name="array"/> has already been disposed.</exception>
		internal Enumerator(RentedArray<T> array)
		{
			ObjectDisposedException.ThrowIf(array._disposed, array);
			_array = array;
			_current = default!;
			_version = array._version;
			_index = -1;
		}

		/// <inheritdoc/>
		[DebuggerStepThrough]
		public void Dispose()
		{
			this = default;
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		/// <see langword="true"/> if the enumerator was successfully advanced to the next element; <see langword="false"/> if the enumerator has passed the end of the collection.
		/// </returns>
		/// <inheritdoc cref="MoveNextRare" path="/exception"/>
		public bool MoveNext()
		{
			int index = _index + 1;
			if (_version == _array._version && (uint)index < (uint)_array.Length)
			{
				_index = index;
				_current = _array._array[_index];
				return true;
			}

			if (_version != _array._version)
			{
				throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");
			}

			_index = _array.Length;
			return false;
		}

		/// <inheritdoc/>
		[DebuggerStepThrough]
		void IEnumerator.Reset()
		{
			_index = -1;
		}
	}
}