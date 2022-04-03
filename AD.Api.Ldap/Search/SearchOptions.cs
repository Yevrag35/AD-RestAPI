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
        private readonly SortOption _sortOption = new();

        [DefaultValue(null)]
        public IFilterStatement? Filter { get; init; }

        [DefaultValue(0)]
        public int PageSize { get; init; }

        public IEnumerable<string>? PropertiesToLoad { get; init; }

        [DefaultValue(SearchScope.Subtree)]
        public SearchScope SearchScope { get; init; } = SearchScope.Subtree;

        [DefaultValue(0)]
        public int SizeLimit { get; init; }

        [DefaultValue(SortDirection.Ascending)]
        public SortDirection SortDirection
        {
            get => _sortOption.Direction;
            init => _sortOption.Direction = value;
        }
        [DefaultValue(null)]
        public string? SortProperty
        {
            get => _sortOption.PropertyName;
            init => _sortOption.PropertyName = value;
        }

        public SortOption GetSortOption() => _sortOption;
    }
}
