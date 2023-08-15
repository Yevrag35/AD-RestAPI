using AD.Api.Ldap.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Search
{
    public interface ISearchOptions
    {
        IFilterStatement? Filter { get; }
        int PageSize { get; }
        IEnumerable<string>? PropertiesToLoad { get; }
        SearchScope SearchScope { get; }
        int SizeLimit { get; }
        SortDirection SortDirection { get; }
        string? SortProperty { get; }

        SortOption GetSortOption();
    }

    public record SearchOptions : ISearchOptions
    {
        protected readonly SortOption SortOption = new();

        [DefaultValue(null)]
        public virtual IFilterStatement? Filter { get; init; }

        [DefaultValue(0)]
        public virtual int PageSize { get; init; }

        public virtual IList<string>? PropertiesToLoad { get; init; }
        IEnumerable<string>? ISearchOptions.PropertiesToLoad => this.PropertiesToLoad;

        [DefaultValue(SearchScope.Subtree)]
        public SearchScope SearchScope { get; init; } = SearchScope.Subtree;

        [DefaultValue(0)]
        public virtual int SizeLimit { get; init; }

        [DefaultValue(SortDirection.Ascending)]
        public SortDirection SortDirection
        {
            get => SortOption.Direction;
            init => SortOption.Direction = value;
        }
        [DefaultValue(null)]
        public string? SortProperty
        {
            get => SortOption.PropertyName;
            init
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    SortOption.PropertyName = value;
                }
            }
        }

        public SortOption GetSortOption() => SortOption;
    }
}
