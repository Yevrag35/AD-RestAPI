using AD.Api.Buffers;

namespace AD.Api.Collections;

public partial class SyncList<T>
{
	/// <summary>
	/// Returns an enumerator that iterates through the live collection.
	/// </summary>
	/// <returns>A <see cref="LiveEnumerator"/> that can be used to iterate through the items in the collection.</returns>
	[DebuggerStepThrough]
	public LiveEnumerator GetEnumerator()
	{
		return new(this);
	}
	/// <inheritdoc/>
	[DebuggerStepThrough]
	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return this.IsThreadSafe
			? new SnapshotEnumerator(this)
			: new NormalEnumerator(this);
	}
	/// <inheritdoc/>
	[DebuggerStepThrough]
	IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<T>)this).GetEnumerator();

	[StructLayout(LayoutKind.Auto)]
	private struct NormalEnumerator : IEnumerator<T>
	{
		private List<T> _list;
		private T _item;
		private int _index;

		public readonly T Current => _item;
		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly object? IEnumerator.Current => this.Current;

		internal NormalEnumerator(SyncList<T> list)
		{
			_list = list._list;
			_item = default!;
			_index = -1;
		}

		public void Dispose()
		{
			this = default;
		}
		public bool MoveNext()
		{
			int next = _index + 1;
			if ((uint)next < (uint)_list.Count)
			{
				_index = next;
				_item = Unsafe.Add(ref MemoryMarshal.GetReference(CollectionsMarshal.AsSpan(_list)), next);
				return true;
			}

			_index = _list.Count;
			_item = default!;
			return false;
		}
		void IEnumerator.Reset()
		{
			_index = -1;
			_item = default!;
		}
	}

	/// <summary>
	/// Enumerates a snapshot of the elements in a <see cref="SyncList{T}"/> at the time the enumerator is created.
	/// </summary>
	/// <remarks>The enumerator captures the contents of the list when it is instantiated, so subsequent
	/// modifications to the underlying <see cref="SyncList{T}"/> do not affect the enumeration. This ensures thread safety
	/// and consistency during iteration. The enumerator must be disposed after use to release any resources associated
	/// with the snapshot.</remarks>
	[StructLayout(LayoutKind.Auto)]
	private struct SnapshotEnumerator : IEnumerator<T>
	{
		private T[] _array;
		private int _index;
		private uint _count;

		public readonly T Current => _array[_index];

		[DebuggerBrowsable(DebuggerBrowsableState.Never)]
		readonly object? IEnumerator.Current => this.Current;

		internal SnapshotEnumerator(SyncList<T> list)
		{
			int count;
			T[] array;
			using (list.EnterScope())
			{
				count = list.Count;
				array = ArrayPool<T>.Shared.Rent(count);
				list._list.CopyTo(array);
			}

			_array = array;
			_count = (uint)count;
			_index = -1;
		}

		public void Dispose()
		{
			T[]? array = _array;
			this = default;
			Rent.Return(array);
		}
		public bool MoveNext()
		{
			int next = _index + 1;
			if ((uint)next < _count)
			{
				_index = next;
				return true;
			}

			_index = (int)_count;
			return false;
		}
		void IEnumerator.Reset()
		{
			_index = -1;
		}
	}

	/// <summary>
	/// Provides a struct-based enumerator for iterating over a synchronized list while detecting modifications during
	/// enumeration.
	/// </summary>
	/// <remarks>A <see cref="LiveEnumerator"/> is typically obtained by calling the <see cref="GetEnumerator"/> method.
	/// The enumerator ensures that the underlying collection is not modified during enumeration; if a
	/// modification is detected, an <see cref="InvalidOperationException"/> is thrown. The enumerator is a ref struct and must be used
	/// within the stack frame in which it was created.</remarks>
	[StructLayout(LayoutKind.Auto)]
	public ref struct LiveEnumerator
	{
		private SyncList<T> _list;
		private T _current;
		private ConditionalLock _gate;
		private int _index;
		private int _count;
		private int _version;

		/// <summary>
		/// Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		/// <remarks>Accessing this property before the enumerator is positioned on a valid element, or after the
		/// collection has been modified, may result in an exception.</remarks>
		/// <inheritdoc cref="ThrowModified" path="/exception"/>
		public readonly T Current => _current;

		internal LiveEnumerator(SyncList<T> list)
		{
			_index = -1;
			_gate = list.EnterScope();
			var innerList = Volatile.Read(in list._list);
			_version = Volatile.Read(ref list._version);
			_count = innerList.Count;
			_list = list;
			_current = default!;
		}
		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <remarks>MoveNext should be called before reading the value of the current element. If the collection is
		/// modified after the enumerator is created, this may throw an exception or return <see langword="false"/>, depending on the
		/// implementation.</remarks>
		/// <returns><see langword="true"/> if the enumerator was successfully advanced to the next element; 
		/// otherwise, <see langword="false"/> if the enumerator has passed the end of the collection.</returns>
		/// <inheritdoc cref="ThrowModified" path="/exception"/>
		public bool MoveNext()
		{
			if (!_gate.IsLocked && _version != _list._version)
				ThrowModified();

			int next = _index + 1;
			if ((uint)next < (uint)_count)
			{
				_index = next;
				_current = _list._list[next];
				return true;
			}

			_index = _count;
			return false;
		}

		public void Dispose()
		{
			_gate.Dispose();
			this = default;
		}

		/// <summary>
		/// Throws an exception to indicate that a collection was modified during enumeration.
		/// </summary>
		/// <exception cref="InvalidOperationException">Always thrown to signal that the underlying collection was changed while it was being enumerated.</exception>
		[DoesNotReturn]
		private static void ThrowModified()
		{
			throw new InvalidOperationException("Collection was modified during enumeration.");
		}
	}
}