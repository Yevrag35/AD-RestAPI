using System.Collections;
using System.Text.Json;

namespace AD.Api.Serialization.Json
{
    [DebuggerDisplay("Count = {Count}")]
    public sealed class JsonDictionary : IDictionary<string, object?>, IReadOnlyDictionary<string, object?>
    {
        private readonly Dictionary<string, object?> _dict;

        public object? this[string key]
        {
            //[DebuggerStepThrough]
            get => _dict[key];
            set => this.SetPair(key, value);
        }

        public int Count
        {
            [DebuggerStepThrough]
            get => _dict.Count;
        }
        public Dictionary<string, object?>.KeyCollection Keys
        {
            [DebuggerStepThrough]
            get => _dict.Keys;
        }
        public Dictionary<string, object?>.ValueCollection Values
        {
            [DebuggerStepThrough]
            get => _dict.Values;
        }

        [DebuggerStepThrough]
        public JsonDictionary(int capacity)
        {
            _dict = new(capacity, StringComparer.OrdinalIgnoreCase);
        }
        public JsonDictionary(IDictionary<string, object?> dictionary)
            : this(dictionary.Count)
        {
            foreach (var kvp in dictionary)
            {
                this.Add(kvp.Key, kvp.Value);
            }
        }

        //[DebuggerStepThrough]
        public void Add(string key, object? value)
        {
            this.AddPair(key, value);
        }
        private void AddPair(string key, object? value)
        {
            value = TransformValue(key, value);
            _dict.Add(key, value);
        }
        [DebuggerStepThrough]
        public bool ContainsKey(string key)
        {
            return _dict.ContainsKey(key);
        }
        [DebuggerStepThrough]
        public void Clear()
        {
            _dict.Clear();
        }
        private static object?[] EnumerateArrayElement(in JsonElement element)
        {
            List<object?> list = new(1);
            foreach (JsonElement item in element.EnumerateArray())
            {
                list.Add(ReadElement(item));
            }

            return list.Count > 0 ? [.. list] : [];
        }
        private static Dictionary<string, object?> EnumerateObjectElement(in JsonElement element)
        {
            Dictionary<string, object?> dict = new(1, StringComparer.OrdinalIgnoreCase);
            foreach (JsonProperty prop in element.EnumerateObject())
            {
                dict.Add(prop.Name, ReadElement(prop.Value));
            }

            return dict;
        }
        [DebuggerStepThrough]
        public IEnumerator<KeyValuePair<string, object?>> GetEnumerator()
        {
            return _dict.GetEnumerator();
        }
        private static object? ReadElement(in JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Undefined:
                case JsonValueKind.Null:
                    goto default;

                case JsonValueKind.Object:
                    return EnumerateObjectElement(in element);

                case JsonValueKind.Array:
                    return EnumerateArrayElement(in element);

                case JsonValueKind.String:
                    return element.GetString();

                case JsonValueKind.Number:
                    string? s = element.GetString();
                    if (int.TryParse(s, out int intRes))
                    {
                        return intRes;
                    }
                    else if (long.TryParse(s, out long longRes))
                    {
                        return longRes;
                    }
                    else if (double.TryParse(s, out double doubleRes))
                    {
                        return doubleRes;
                    }

                    return s;

                case JsonValueKind.True:
                case JsonValueKind.False:
                    return element.GetBoolean();

                default:
                    return null;
            }
        }
        [DebuggerStepThrough]
        public bool Remove(string key)
        {
            return _dict.Remove(key);
        }
        private void SetPair(string key, object? value)
        {
            value = TransformValue(key, value);
            _dict[key] = value;
        }
        private static object? TransformValue(string key, object? value)
        {
            if (value is null)
            {
                return value;
            }
            
            if (value is not JsonElement element)
            {
                return value;
            }

            return ReadElement(in element);
        }
        [DebuggerStepThrough]
        public bool TryGetValue(string key, [MaybeNull] out object? value)
        {
            return _dict.TryGetValue(key, out value);
        }
        public bool TryGetValueAs<T>(string key, [NotNullWhen(true)] out T? value)
        {
            if (this.TryGetValue(key, out object? objValue) && objValue is T tVal)
            {
                value = tVal;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        #region INTERFACE EXPLICITS
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        bool ICollection<KeyValuePair<string, object?>>.IsReadOnly
        {
            [DebuggerStepThrough]
            get => false;
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection<string> IDictionary<string, object?>.Keys
        {
            [DebuggerStepThrough]
            get => this.Keys;
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<string> IReadOnlyDictionary<string, object?>.Keys
        {
            [DebuggerStepThrough]
            get => this.Keys;
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        ICollection<object?> IDictionary<string, object?>.Values
        {
            [DebuggerStepThrough]
            get => this.Values;
        }
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        IEnumerable<object?> IReadOnlyDictionary<string, object?>.Values
        {
            [DebuggerStepThrough]
            get => this.Values;
        }

        [DebuggerStepThrough]
        void ICollection<KeyValuePair<string, object?>>.Add(KeyValuePair<string, object?> item)
        {
            this.Add(item.Key, item.Value);
        }

        [DebuggerStepThrough]
        bool ICollection<KeyValuePair<string, object?>>.Contains(KeyValuePair<string, object?> item)
        {
            return this.ContainsKey(item.Key);
        }

        [DebuggerStepThrough]
        void ICollection<KeyValuePair<string, object?>>.CopyTo(KeyValuePair<string, object?>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, object?>>)_dict).CopyTo(array, arrayIndex);
        }

        [DebuggerStepThrough]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        [DebuggerStepThrough]
        bool ICollection<KeyValuePair<string, object?>>.Remove(KeyValuePair<string, object?> item)
        {
            return this.Remove(item.Key);
        }

        #endregion
    }
}

