using System.Collections;

namespace AD.Api.Startup.Services.Internal
{
    internal sealed class ServiceTypeExclusions : IAddServiceTypeExclusions, IServiceTypeExclusions
    {
        private readonly HashSet<Type> _explicitExclusions;
        private readonly HashSet<Type> _genericDefinitions;

        /// <summary>
        /// The total number of registered exclusion types whether they are explicit or generic definitions.
        /// </summary>
        public int Count => _explicitExclusions.Count + _genericDefinitions.Count;

        private ServiceTypeExclusions()
        {
            _explicitExclusions = [];
            _genericDefinitions = [];
        }

        /// <inheritdoc/>
        public IAddServiceTypeExclusions Add<T>()
        {
            return this.Add(typeof(T));
        }
        /// <inheritdoc/>
        public IAddServiceTypeExclusions Add(Type type)
        {
            ArgumentNullException.ThrowIfNull(type);
            if (type.IsGenericTypeDefinition)
            {
                _genericDefinitions.Add(type);
            }
            else
            {
                _explicitExclusions.Add(type);
            }

            return this;
        }
        /// <inheritdoc/>
        public bool Contains<T>()
        {
            return this.Contains(typeof(T));
        }
        /// <inheritdoc/>
        public bool Contains(Type type)
        {
            return type.IsGenericTypeDefinition
                ? _genericDefinitions.Contains(type)
                : _explicitExclusions.Contains(type);
        }
        private bool ContainsDefinitionFor(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            Type genDef = type.GetGenericTypeDefinition();
            return _genericDefinitions.Contains(genDef);
        }
        /// <inheritdoc/>
        public bool IsExcluded(Type type)
        {
            return this.Contains(type) || this.ContainsDefinitionFor(type);
        }

        /// <summary>
        /// Returns an iterator for all the types and type definitions that have been specified for exclusion from 
        /// service registration.
        /// </summary>
        /// <returns>
        ///     An <see cref="IEnumerator{T}"/> object that can be used to iterate through the exclusion sets.
        /// </returns>
        public IEnumerator<Type> GetEnumerator()
        {
            IEnumerable<Type> allTypes = _genericDefinitions;
            allTypes = allTypes.Concat(_explicitExclusions);

            return allTypes.GetEnumerator();
        }
        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal static IServiceTypeExclusions ConfigureFromAction(Action<IAddServiceTypeExclusions>? configure)
        {
            if (configure is null)
            {
                return default(EmptyExclusions);
            }

            ServiceTypeExclusions exclusions = new();
            configure(exclusions);

            return exclusions;
        }

        private readonly struct EmptyExclusions : IServiceTypeExclusions
        {
            public readonly int Count => 0;

            public readonly bool Contains<T>()
            {
                return false;
            }
            public readonly bool Contains(Type type)
            {
                return false;
            }
            public readonly IEnumerator<Type> GetEnumerator()
            {
                return Enumerable.Empty<Type>().GetEnumerator();
            }
            public readonly bool IsExcluded(Type type)
            {
                return false;
            }
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}

