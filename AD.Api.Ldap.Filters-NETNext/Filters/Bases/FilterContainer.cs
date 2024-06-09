using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Collections;
using System.Text;

namespace AD.Api.Ldap.Filters
{
    /// <summary>
    /// The <see langword="abstract"/> base record for LDAP filter statements that contain sub-statements.
    /// </summary>
#if OLDCODE
    public abstract class FilterContainer : FilterStatementBase, IEnumerable<IFilterStatement>
#else
    public abstract record FilterContainer : FilterStatementBase, IEnumerable<IFilterStatement>
#endif
    {
        private static readonly Type _filterStatementType = typeof(IFilterStatement);

        /// <summary>
        /// The list of sub-statements that the <see cref="FilterContainer"/> contains.
        /// </summary>
        protected List<IFilterStatement> Clauses { get; }

        /// <summary>
        /// The number of sub-statements contained in the <see cref="FilterContainer"/>.
        /// </summary>
        public int Count => this.Clauses.Count;

        public override int Length => this.Clauses.Sum(x => x.Length);

        /// <summary>
        /// Initializes an instance of <see cref="FilterContainer"/> with the default initial capacity.
        /// </summary>
        public FilterContainer()
            : this(0)
        {
        }

        /// <summary>
        /// Initializes an instance of <see cref="FilterContainer"/> with the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///     The number of sub-statements the <see cref="FilterContainer"/> can initially hold.
        /// </param>
        public FilterContainer(int capacity)
        {
            this.Clauses = new List<IFilterStatement>(capacity);
        }

        /// <summary>
        /// Adds a sub statement to the <see cref="FilterContainer"/>.
        /// </summary>
        /// <remarks>
        ///     If <paramref name="statement"/> is <see langword="null"/>, it will not be added
        ///     to the <see cref="FilterContainer"/>.
        /// </remarks>
        /// <param name="statement">The filter statement to add.</param>
        public virtual void Add(IFilterStatement? statement)
        {
            if (statement is not null)
            {
                this.Clauses.Add(statement);
            }
        }
        /// <summary>
        /// Determines wheter the <see cref="FilterContainer"/> contains an <see cref="IFilterStatement"/>
        /// of the specified type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="IFilterStatement"/> to check for.</typeparam>
        /// <returns>
        ///     <see langword="true"/> if at least 1 <see cref="IFilterStatement"/> of the the type <typeparamref name="T"/>
        ///     is in the <see cref="FilterContainer"/>; otherwise, <see langword="false"/>.
        /// </returns>
        protected bool Contains<T>() where T : IFilterStatement
        {
            return this.Clauses.Exists(statement => statement is T);
        }
        protected bool Contains<T>(int count) where T : IFilterStatement
        {
            return this.Clauses.Count >= count && this.Clauses.Count(statement => statement is T) == count;
        }
        protected bool Contains(Type typeOfFilterStatement)
        {
            if (!_filterStatementType.IsAssignableFrom(typeOfFilterStatement))
            {
                throw new ArgumentException($"{nameof(typeOfFilterStatement)} must implement '{nameof(IFilterStatement)}'.");
            }

            return this.Clauses.Exists(statement => typeOfFilterStatement.Equals(statement.GetType()));
        }
        /// <summary>
        /// Determines whether the specified statement is in the <see cref="FilterContainer"/>.
        /// </summary>
        /// <param name="statement">The statement to locate.</param>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="statement"/> is found;
        ///     otherwise, <see langword="false"/>.
        /// </returns>
        public virtual bool Contains(IFilterStatement statement)
        {
            return this.Clauses.Contains(statement);
        }
        protected void Clear() => this.Clauses.Clear();
        protected bool Exists<T>(Predicate<T> predicate) where T : IFilterStatement
        {
            return this.Clauses.Exists(clause => clause is T tClause && predicate(tClause));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see cref="FilterContainer"/>.
        /// </summary>
        /// <returns>
        ///     An <see cref="IEnumerator{T}"/> for the <see cref="FilterContainer"/>.
        /// </returns>
        public IEnumerator<IFilterStatement> GetEnumerator() => this.Clauses.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

        /// <summary>
        /// Removes the first occurrence of the specified <see cref="IFilterStatement"/> found in the 
        /// <see cref="FilterContainer"/>.
        /// </summary>
        /// <param name="statement">The statement to remove.</param>
        /// <returns>
        ///     <see langword="true"/> if <paramref name="statement"/> was removed successfully;
        ///     otherwise, <see langword="false"/>.  This method also returns <see langword="false"/> 
        ///     if <paramref name="statement"/> is <see langword="null"/> or not found in the 
        ///     <see cref="FilterContainer"/>.
        /// </returns>
        public bool Remove(IFilterStatement statement) => this.Clauses.Remove(statement);
        protected int RemoveAll(Predicate<IFilterStatement> predicate)
        {
            return this.Clauses.RemoveAll(predicate);
        }
        protected int RemoveAll<T>(Predicate<T> predicate) where T : IFilterStatement
        {
            return this.RemoveAll(clause => clause is T tClause && predicate(tClause));
        }

        public sealed override bool Equals(IFilterStatement? other)
        {
            if (base.Equals(other) && other is FilterContainer filCon && this.Count == filCon.Count)
            {
                for (int i = 0; i < this.Clauses.Count; i++)
                {
                    if (!this.Clauses[i].Equals(filCon.Clauses[i]))
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        public override void WriteTo(JsonWriter writer, NamingStrategy strategy, JsonSerializer serializer)
        {
            foreach (IFilterStatement clause in this.Clauses)
            {
                clause.WriteTo(writer, strategy, serializer);
            }
        }
        public override StringBuilder WriteTo(StringBuilder builder)
        {
            foreach (IFilterStatement clause in this.Clauses)
            {
                _ = clause.WriteTo(builder);
            }

            return builder;
        }
    }
}
