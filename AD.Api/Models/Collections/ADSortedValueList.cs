using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AD.Api.Models.Collections
{
    public class ADSortedValueList<T> : IValueCollection<T>
    {
        private Func<T, object> _keySelector;
        private SortedList<object, T> InnerList;
        private static readonly Func<T, T, int> _func = (str1, str2) =>
        {
            return StringComparer.CurrentCultureIgnoreCase.Compare(str1, str2);
        };

        public T this[int index]
        {
            get => this.InnerList.Values[index];
            set
            {
                if (this.InnerList.ContainsKey(value))
                    return;

                T item = this[index];
                this.InnerList.Add(value, value);
            }
        }

        public int Count => this.InnerList.Count;
        public bool EnforcesUnique => true;
        public bool IsReadOnly => false;
        public bool SortsAlways => true;

        public ADSortedValueList()
            : this(0, x => x, null)
        {
        }
        public ADSortedValueList(int capacity)
            : this(capacity, x => x, null)
        {
        }
        public ADSortedValueList(Func<T, object> keySelector)
            : this(0, keySelector, null)
        {
        }
        public ADSortedValueList(Func<T, T, int> comparer)
            : this(0, x => x, comparer)
        {
        }
        public ADSortedValueList(int capacity, Func<T, object> keySelector)
            : this(capacity, keySelector, null)
        {
        }
        public ADSortedValueList(int capacity, Func<T, T, int> comparer)
            : this(capacity, x => x, comparer)
        {
        }
        public ADSortedValueList(int capacity, Func<T, object> keySelector, Func<T, T, int> comparer)
        {
            _keySelector = keySelector;
            if (typeof(T).Equals(typeof(string)))
                this.InnerList = new SortedList<object, T>(capacity, new GenericKeyComparer(_func));

            else
                this.InnerList = new SortedList<object, T>(capacity, new GenericKeyComparer(comparer));
        }

        private class GenericKeyComparer : IComparer<object>
        {
            private Func<T, T, int> _compareFunc;

            public GenericKeyComparer(Func<T, T, int> comparer)
            {
                _compareFunc = comparer;
            }

            public int Compare(object x, object y)
            {
                if (x is T xT && y is T yT)
                {
                    return _compareFunc != null
                        ? _compareFunc(xT, yT)
                        : Comparer<T>.Default.Compare(xT, yT);
                }

                return Comparer<object>.Default.Compare(x, y);
            }
        }

        public virtual void Add(T item)
        {
            object key = _keySelector(item);
            if (this.InnerList.ContainsKey(key))
                return;

            this.InnerList.Add(key, item);
        }
        public virtual void AddRange(IEnumerable<T> values)
        {
            foreach (T value in values)
            {
                this.Add(value);
            }
        }
        public void Clear()
        {
            this.InnerList.Clear();
        }
        public virtual bool Contains(T item)
        {
            object key = _keySelector(item);
            return this.InnerList.ContainsKey(key);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            this.InnerList.Values.CopyTo(array, arrayIndex);
        }
        public IEnumerator<T> GetEnumerator()
        {
            return this.InnerList.Values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        public virtual int IndexOf(T item)
        {
            return this.InnerList.IndexOfValue(item);
        }
        public virtual void Insert(int index, T item)
        {
            this.Add(item);
        }
        public virtual bool Remove(T item)
        {
            object key = _keySelector(item);
            return this.InnerList.Remove(key);
        }
        public virtual void RemoveAt(int index)
        {
            this.InnerList.RemoveAt(index);
        }
    }
}
