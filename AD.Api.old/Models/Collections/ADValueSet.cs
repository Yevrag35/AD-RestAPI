using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AD.Api.Models.Collections
{
    /// <summary>
    /// Provides an <see cref="IList{T}"/> implementing class that enforces items to be unique.
    /// </summary>
    public class ADValueSet<T> : IValueCollection<T>, IList
    {
        #region FIELDS/CONSTANTS
        /// <summary>
        /// The internal, backing <see cref="List{T}"/> collection that all methods invoke against.
        /// </summary>
        protected private List<T> InnerList;
        /// <summary>
        /// The internal, backing <see cref="HashSet{T}"/> set that determines uniqueness in the <see cref="ADValueSet{T}"/>.
        /// </summary>
        private HashSet<T> InnerSet;

        #endregion

        #region INDEXERS
        public T this[int index]
        {
            get => this.InnerList[index];
            set
            {
                T item = this.InnerList[index];
                if (this.InnerSet.Add(value))
                {
                    this.InnerSet.Remove(item);
                    this.InnerList[index] = value;
                }
            }
        }

        #endregion

        #region PROPERTIES
        /// <summary>
        /// The equality comparer used to determine uniqueness in the list./>.
        /// </summary>
        public IEqualityComparer<T> Comparer => InnerSet.Comparer;

        /// <summary>
        /// Get the number of elements contained within the <see cref="ADValueSet{T}"/>.
        /// </summary>
        public int Count => InnerList.Count;

        public bool EnforcesUnique => true;
        public bool SortsAlways => false;

        #endregion

        #region CONSTRUCTORS
        /// <summary>
        /// Initializes a new instance of the <see cref="ADValueSet{T}"/> class that is empty
        /// and has the default initial capacity.
        /// </summary>
        public ADValueSet()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ADValueSet{T}"/> class that is empty
        /// and has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">The number of elements that the new collection can initially store.</param>
        /// <exception cref="ArgumentOutOfRangeException"/>
        public ADValueSet(int capacity)
            : this(capacity, GetDefaultComparer())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ADValueSet{T}"/> class that
        /// contains elements copied from the specified <see cref="IEnumerable{T}"/> and has
        /// sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        /// <exception cref="ArgumentNullException"/>
        public ADValueSet(IEnumerable<T> collection)
            : this(collection, GetDefaultComparer())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="equalityComparer">
        ///     The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the list, or
        ///     <see langword="null"/> to use the default <see cref="EqualityComparer{T}"/> implementation for the
        ///     type <typeparamref name="T"/>.
        /// </param>
        public ADValueSet(IEqualityComparer<T> equalityComparer)
            : this(0, equalityComparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ADValueSet{T}"/> class that is empty, has the specified
        /// initial capacity, and uses the specified equality comparer for the <typeparamref name="T"/> type.
        /// </summary>
        /// <param name="capacity">The number of elements that the new collection can initially store.</param>
        /// <param name="equalityComparer">
        ///     The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the list, or
        ///     <see langword="null"/> to use the default <see cref="EqualityComparer{T}"/> implementation for the
        ///     type <typeparamref name="T"/>.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="capacity"/> is less than 0.</exception>
        public ADValueSet(int capacity, IEqualityComparer<T> equalityComparer)
        {
            InnerList = new List<T>(capacity);
            InnerSet = new HashSet<T>(equalityComparer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ADValueSet{T}"/> class that uses the specified comparer for 
        /// the <typeparamref name="T"/> type, contains elements copied from the specified collection, and sufficient capacity
        /// to accommodate the number of elements copied.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new list.</param>
        /// <param name="equalityComparer">
        ///     The <see cref="IEqualityComparer{T}"/> implementation to use when comparing values in the list, or
        ///     <see langword="null"/> to use the default <see cref="EqualityComparer{T}"/> implementation for the
        ///     type <typeparamref name="T"/>.
        /// </param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
        public ADValueSet(IEnumerable<T> collection, IEqualityComparer<T> equalityComparer)
        {
            InnerSet = new HashSet<T>(collection, equalityComparer);
            InnerList = new List<T>(InnerSet);
        }

        #endregion

        #region BASE METHODS
        /// <summary>
        /// Adds an item to the end of the collection.
        /// </summary>
        /// <param name="item">The object to be added to the end of the collection.</param>
        public virtual void Add(T item)
        {
            _ = this.Add(item, true);
        }

        public void AddRange(IEnumerable<T> items)
        {
            foreach (T value in items)
            {
                _ = this.Add(value, true);
            }
        }

        //public int AddInitial(IEnumerable<T> items)
        //{
        //    if (InnerList.Count > 0 || InnerSet.Count > 0)
        //        return 0;

        //    int added = 0;
        //    foreach (T item in items)
        //    {
        //        if (InnerSet.Add(item))
        //        {
        //            InnerList.Add(item);
        //            added++;
        //        }
        //    }

        //    return added;
        //}
        /// <summary>
        /// Removes all elements from the <see cref="ADValueSet{T}"/>.
        /// </summary>
        public void Clear()
        {
            this.Clear(true);
        }
        /// <summary>
        /// Determines whether an element is in the <see cref="ADValueSet{T}"/>.
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="ADValueSet{T}"/>.  The value can be null for reference types.
        /// </param>
        public bool Contains(T item)
        {
            return this.Contains(item, true);
        }
        /// <summary>
        /// Copies the entire <see cref="ADValueSet{T}"/> to a compatible one-dimensional array, starting at
        /// the specified index of the target array.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional array that is the destination of the elements copied from
        /// <see cref="ADValueSet{T}"/>.  The array must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">The zero-based index in the target array at which copying begins.</param>
        /// <exception cref="ArgumentNullException"/>
        /// <exception cref="ArgumentOutOfRangeException"/>
        /// <exception cref="ArgumentException"/>
        public void CopyTo(T[] array, int arrayIndex) => InnerList.CopyTo(array, arrayIndex);

        public void ExceptWith(IEnumerable<T> items)
        {
            if (null == items)
                return;

            foreach (T item in items)
            {
                _ = this.Remove(item);
            }
        }

        public void ForEach(Action<T> action)
        {
            this.InnerList.ForEach(action);
        }

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence
        /// within the entire <see cref="ADValueSet{T}"/>.
        /// </summary>
        /// <param name="item">The object to locate in the <see cref="ADValueSet{T}"/>.  The value can be null for reference types.</param>
        public int IndexOf(T item)
        {
            return this.IndexOf(item, true);
        }

        public void Insert(int index, T item)
        {
            this.Insert(index, item, true);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ADValueSet{T}"/>.  The
        /// value can be null for reference types.
        /// </summary>
        /// <param name="item">
        /// The object to remove from the <see cref="ADValueSet{T}"/>.
        /// The value can be null for reference types.
        /// </param>
        public bool Remove(T item)
        {
            return this.Remove(item, true);
        }

        public void RemoveAt(int index)
        {
            T item = default;
            try
            {
                item = InnerList[index];
            }
            catch (ArgumentOutOfRangeException)
            {
                return;
            }

            _ = this.Remove(item, true);
        }

        #endregion

        #region INTERFACE EXPLICIT MEMBERS
        object IList.this[int index]
        {
            get => InnerList[index];
            set
            {
                if (value is T item)
                    InnerList[index] = item;
            }
        }

        bool IList.IsFixedSize => false;
        bool IList.IsReadOnly => false;

        int IList.Add(object value)
        {
            return value is T item && this.Add(item, true)
                ? this.IndexOf(item)
                : -1;
        }
        bool IList.Contains(object value)
        {
            return value is T item && this.Contains(item);
        }
        int IList.IndexOf(object value)
        {
            return value is T item
                ? this.IndexOf(item)
                : -1;
        }
        void IList.Insert(int index, object value)
        {
            if (value is T item)
                this.Insert(index, item);
        }
        void IList.Remove(object value)
        {
            if (value is T item)
                this.Remove(item);
        }

        #endregion

        #region ENUMERATOR
        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="ADValueSet{T}"/>.
        /// </summary>
        public IEnumerator<T> GetEnumerator() => InnerList.GetEnumerator();
        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="IEnumerable"/>.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator() => InnerList.GetEnumerator();

        #endregion

        #region INTERFACE MEMBERS

        #region IMPLEMENTED INTERFACE PROPERTIES
        bool ICollection<T>.IsReadOnly => ((ICollection<T>)InnerList).IsReadOnly;
        bool ICollection.IsSynchronized => ((ICollection)InnerList).IsSynchronized;
        object ICollection.SyncRoot => ((ICollection)InnerList).SyncRoot;

        #endregion

        #region IMPLEMENTED INTERFACE METHODS
        void ICollection<T>.Add(T item) => this.Add(item);
        void ICollection.CopyTo(Array array, int index) => ((ICollection)InnerList).CopyTo(array, index);

        #endregion

        #endregion

        #region BACKEND/PRIVATE METHODS
        protected virtual bool Add(T item, bool adding)
        {
            if (adding)
            {
                bool result = false;
                if (InnerSet.Add(item))
                {
                    InnerList.Add(item);
                    result = true;
                }

                return result;
            }
            else
                return adding;
        }

        protected virtual void Clear(bool clearing)
        {
            if (clearing)
            {
                InnerList.Clear();
                InnerSet.Clear();
            }
        }

        protected virtual bool Contains(T item, bool querying)
        {
            return querying && this.InnerSet.Contains(item);
        }
        
        protected virtual int IndexOf(T item, bool querying)
        {
            int index = -1;
            if (querying)
            {
                index = this.InnerList.IndexOf(item);
            }

            return index;
        }

        protected virtual void Insert(int index, T item, bool inserting)
        {
            if (inserting && InnerSet.Add(item))
            {
                try
                {
                    InnerList.Insert(index, item);
                }
                catch (ArgumentOutOfRangeException)
                {
                    InnerSet.Remove(item);
                }
            }
        }

        protected virtual bool Remove(T item, bool removing)
        {
            return removing && InnerSet.Remove(item)
                ? InnerList.Remove(item)
                : false;
        }

        protected virtual bool ReplaceValueAtIndex(int index, T newValue)
        {
            bool result = false;
            T item = InnerList[index];

            if (InnerSet.Add(newValue))
            {
                result = InnerSet.Remove(item);
                InnerList[index] = newValue;
            }

            return result;
        }

        private static IEqualityComparer<T> GetDefaultComparer()
        {
            if (typeof(T).Equals(typeof(string)))
                return (IEqualityComparer<T>)StringComparer.CurrentCultureIgnoreCase;

            else
                return EqualityComparer<T>.Default;
        }

        #endregion

    }
}