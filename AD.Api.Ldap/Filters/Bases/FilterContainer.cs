using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AD.Api.Ldap.Filters
{
    public abstract record FilterContainer : FilterStatementBase, IEnumerable<IFilterStatement>
    {
        protected List<IFilterStatement> Clauses { get; }

        public int Count => this.Clauses.Count;

        public FilterContainer()
            : this(0)
        {
        }
        public FilterContainer(int capacity)
        {
            this.Clauses = new List<IFilterStatement>(capacity);
        }

        public virtual void Add(IFilterStatement? statement)
        {
            if (statement is not null)
                this.Clauses.Add(statement);
        }
        public bool Contains<T>() where T : IFilterStatement
        {
            return this.Clauses.Exists(statement => statement is T);
        }
        public bool Contains<T>(int count) where T : IFilterStatement
        {
            return this.Clauses.Count >= count && this.Clauses.Count(statement => statement is T) == count;
        }
        public bool Contains(IFilterStatement statement)
        {
            return this.Clauses.Contains(statement);
        }
        public void Clear() => this.Clauses.Clear();
        public bool Exists<T>(Predicate<T> predicate) where T : IFilterStatement
        {
            return this.Clauses.Exists(clause => clause is T tClause && predicate(tClause));
        }
        public IEnumerator<IFilterStatement> GetEnumerator() => this.Clauses.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
        public bool Remove(IFilterStatement statement) => this.Clauses.Remove(statement);
        public int RemoveAll(Predicate<IFilterStatement> predicate)
        {
            return this.Clauses.RemoveAll(predicate);
        }
        public int RemoveAll<T>(Predicate<T> predicate) where T : IFilterStatement
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

        public override StringBuilder WriteTo(StringBuilder builder)
        {
            this.Clauses.ForEach(clause =>
            {
                clause.WriteTo(builder);
            });

            return builder;
        }
    }
}
