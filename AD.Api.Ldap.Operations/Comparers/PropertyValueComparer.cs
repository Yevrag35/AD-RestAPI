using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace AD.Api.Ldap.Operations
{
    public class PropertyValueComparer : IEqualityComparer<object>, IEqualityComparer<string>
    {
        private static readonly StringComparer _stringComparer = StringComparer.CurrentCultureIgnoreCase;

        public IEqualityComparer Equality { get; }

        public PropertyValueComparer(Type comparingType)
        {
            this.Equality = comparingType.Equals(typeof(string))
                ? _stringComparer
                : GetEqualityComparer(comparingType);
        }

        bool IEqualityComparer<object>.Equals(object? x, object? y)
        {
            return this.Equality.Equals(x, y);
        }
        int IEqualityComparer<object>.GetHashCode([DisallowNull] object obj)
        {
            return this.Equality.GetHashCode(obj);
        }

        public bool Equals(string? x, string? y)
        {
            return _stringComparer.Equals(x, y);
        }
        public int GetHashCode([DisallowNull] string obj)
        {
            return _stringComparer.GetHashCode(obj);
        }

        private IEqualityComparer GetEqualityComparer(Type forType)
        {
            MethodInfo? method = this.GetType()
                .GetMethod(nameof(GetGenericEqualityComparer), BindingFlags.NonPublic | BindingFlags.Static)
                ?.MakeGenericMethod(forType);

            return !(method is null) && TryExecute(method, out IEqualityComparer? equalityComparer)
                ? equalityComparer
                : (IEqualityComparer)EqualityComparer<object>.Default;
        }

        private static EqualityComparer<T> GetGenericEqualityComparer<T>()
        {
            return EqualityComparer<T>.Default;
        }
        
        private static bool TryExecute(MethodInfo method, 
            [NotNullWhen(true)] out IEqualityComparer? equalityComparer)
        {
            equalityComparer = (IEqualityComparer?)method.Invoke(null, Array.Empty<object>());

            return !(equalityComparer is null);
        }
    }
}
