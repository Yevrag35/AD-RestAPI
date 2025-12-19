using AD.Api.Validation;
using System.ComponentModel;

namespace AD.Api.Buffers;

/// <summary>
/// Provides static factory methods for creating <see cref="ArraySlice{T}"/> instances.
/// </summary>
public static class ArraySlice
{
	public static ArraySlice<T> Create<T>(params ReadOnlySpan<T> values)
	{
		if (values.IsEmpty)
			return Empty<T>();

		return new(values.ToArray(), 0, values.Length);
	}
	/// <summary>
	/// Returns an empty slice of the specified array element type.
	/// </summary>
	/// <remarks>The returned slice has a length of zero. This instance
	/// can be reused wherever an empty slice is required.</remarks>
	/// <typeparam name="T">The type of elements in the array slice.</typeparam>
	/// <returns>An <see cref="ArraySlice{T}"/> instance over a span of zero elements.</returns>
	public static ArraySlice<T> Empty<T>() => EmptyInstance<T>.Default;

	private static class EmptyInstance<T>
	{
		internal static readonly ArraySlice<T> Default = new([]);
	}
}

/// <summary>
/// Represents a contiguous slice of an array, defined by an offset and length, without copying the underlying data.
/// </summary>
/// <remarks><see cref="ArraySlice{T}"/> provides a lightweight view over a segment of an array, allowing efficient access and
/// manipulation of a subset of its elements. The slice does not own the array; changes to the underlying array are
/// reflected in the slice. This type is useful for scenarios where working with subarrays is required without incurring
/// the cost of allocation or copying. <see cref="ArraySlice{T}"/> is a value type and is intended for performance-critical code. It
/// is not thread-safe if the underlying array is modified concurrently.</remarks>
/// <typeparam name="T">The type of elements contained in the array slice.</typeparam>
[StructLayout(LayoutKind.Sequential)]
[DebuggerStepThrough, DebuggerDisplay("Length = {Length}")]
[CollectionBuilder(typeof(ArraySlice), nameof(ArraySlice.Create))]
public readonly struct ArraySlice<T> //: IReadOnlyCollection<T>
{
	private readonly T[]? _array;
	private readonly int _length;
	private readonly int _offset;

	/// <summary>
	/// Gets the underlying array represented by this slice.
	/// </summary>
	/// <value>
	/// A reference to the underlying array, or an empty array if the slice is empty
	/// or default-initialized.
	/// </value>
	public T[] Array => _array ?? [];
	/// <summary>
	/// Gets the number of elements in the slice.
	/// </summary>
	public readonly int Length => _length;
	/// <summary>
	/// Gets the zero-based offset in the underlying array where the slice begins.
	/// </summary>
	public readonly int Offset => _offset;
	//int IReadOnlyCollection<T>.Count => _length;

	/// <summary>
	/// Initializes a new instance of the ArraySlice class that represents an empty slice over the specified array.
	/// </summary>
	/// <remarks>This constructor is intended for internal use to create an ArraySlice that represents an empty
	/// collection. The provided array must have a length of zero.</remarks>
	/// <param name="empty">An array that must be empty. Used as the underlying storage for the empty slice.</param>
	internal ArraySlice(T[] empty)
	{
		Debug.Assert(empty.Length == 0);
		_array = empty;
		_length = 0;
		_offset = 0;
	}
	/// <summary>
	/// Initializes a new instance of the ArraySlice<T> class that represents a slice of the specified array, starting at
	/// the beginning and containing the specified number of elements.
	/// </summary>
	/// <param name="array">The array to create the slice from. Cannot be null.</param>
	/// <param name="length">The number of elements to include in the slice. Must be non-negative and not greater than the length of the array.</param>
	public ArraySlice(T[] array, int length) : this(array, 0, length)
	{
	}
	/// <summary>
	/// Initializes a new instance of the ArraySlice class that represents a contiguous segment of the specified array.
	/// </summary>
	/// <param name="array">The array to create a slice from. Cannot be null.</param>
	/// <param name="offset">The zero-based index in the array at which the slice begins. Must be greater than or equal to 0 and less than or
	/// equal to the length of the array.</param>
	/// <param name="length">The number of elements in the slice. Must be non-negative and not exceed the number of elements from offset to the
	/// end of the array.</param>
	public ArraySlice(T[] array, int offset, int length)
	{
		ArgumentNullException.ThrowIfNull(array);
		ThrowHelper.ThrowIfNegativeOrGreaterThan(length, (uint)array.Length - (uint)offset, nameof(length));
		_array = array;
		_offset = offset;
		_length = length;
	}

	[DebuggerStepThrough, EditorBrowsable(EditorBrowsableState.Never)]
	public void Deconstruct(out T[] array, out int length, out int offset)
	{
		array = this.Array;
		length = _length;
		offset = _offset;
	}

	/// <summary>
	/// Returns a read-only span over the valid segment of the underlying array.
	/// </summary>
	/// <remarks>The returned span reflects the current state of the underlying array segment. Modifications to the
	/// array after obtaining the span are visible through the span. The span does not allocate memory.</remarks>
	/// <returns>A <see cref="ReadOnlySpan{T}"/> representing the elements in the current segment. Returns an empty span if the
	/// segment is empty or the underlying array is null.</returns>
	public ReadOnlySpan<T> AsSpan()
	{
		return _array is T[] array && array.Length > 0
			? array.AsSpan(_offset, _length)
			: [];
	}

	public static implicit operator ReadOnlySpan<T>(ArraySlice<T> slice)
	{
		return slice._array is T[] array && array.Length > 0
			? new ReadOnlySpan<T>(array, slice._offset, slice._length)
			: [];
	}
	public static implicit operator Span<T>(ArraySlice<T> slice)
	{
		return slice._array is T[] array && array.Length > 0
			? array.AsSpan(slice._offset, slice._length)
			: [];
	}

	/// <summary>
	/// Returns an enumerator that iterates through the <see cref="ArraySlice{T}"/>
	/// </summary>
	/// <returns>An enumerator that can be used to iterate through the contiguous slice.</returns>
	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}
	//IEnumerator<T> IEnumerable<T>.GetEnumerator()
	//{
	//	return new Enumerator(this);
	//}
	//IEnumerator IEnumerable.GetEnumerator()
	//{
	//	return new Enumerator(this);
	//}

	/// <summary>
	/// Supports iteration over the elements of an <see cref="ArraySlice{T}"/>
	/// </summary>
	/// <remarks>The <see cref="Enumerator"/> is a ref struct and cannot be stored on the managed heap, boxed, or used across
	/// await or yield boundaries. It is typically used in a foreach statement to enumerate the elements of an
	/// <see cref="ArraySlice{T}"/> in order.</remarks>
	[StructLayout(LayoutKind.Auto)]
	public ref struct Enumerator //: IEnumerator<T>
	{
		private T[] _array;
		private T _current;
		private int _index;
		private int _length;
		private int _offset;

		internal Enumerator(ArraySlice<T> slice)
		{
			_array = slice.Array;
			_current = default!;
			_index = -1;
			_length = slice.Length;
			_offset = slice.Offset;
		}

		public readonly T Current => _current;

		public bool MoveNext()
		{
			int next = _index + 1;
			if ((uint)next >= (uint)_length)
			{
				return false;
			}

			_current = _array[_offset + next];
			_index = next;
			return true;
		}
		public void Reset()
		{
			_index = -1;
			_current = default!;
		}
		public void Dispose()
		{
			this = default;
		}
	}
}
