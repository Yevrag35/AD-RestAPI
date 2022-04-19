using AD.Api.Ldap.Attributes;
using AD.Api.Ldap.Filters;
using AD.Api.Ldap.Mapping;
using AD.Api.Ldap.Models;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AD.Api.Ldap.Search
{
    public interface ILdapSearcher : IDisposable
    {
        List<FindResult> FindAll(ISearchOptions searchOptions, out string ldapFilter, out string? hostContacted);
    }

    internal sealed class Searcher : ILdapSearcher
    {
        private bool _disposed;
        //private readonly StringBuilder _builder;
        private readonly Lazy<StringBuilder> _lazyBuilder = new();
        private readonly DirectorySearcher _searcher;
        private readonly ILdapEnumDictionary _enumDictionary;
        public IFilterStatement? Filter { get; set; }

        internal Searcher(LdapConnection connection, ILdapEnumDictionary enumDictionary, ISearchOptions? options = null)
            : this(connection.GetSearchBase(), enumDictionary, options)
        {
        }
        internal Searcher(DirectoryEntry searchBase, ILdapEnumDictionary enumDictionary, ISearchOptions? options = null)
        {
            _enumDictionary = enumDictionary;
            _searcher = new DirectorySearcher(searchBase);
            this.Filter = options?.Filter;
            if (options is not null)
                _ = SetSearcher(options, _lazyBuilder.Value, _searcher);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _searcher.Dispose();
            if (_lazyBuilder.IsValueCreated)
                _lazyBuilder.Value.Clear();

            _disposed = true;

            GC.SuppressFinalize(this);
        }

        //public FindResult? FindOne()
        //{
        //    _builder.Clear();
        //    _searcher.Filter = this.Filter?.WriteTo(_builder).ToString();
        //    SearchResult? result = _searcher.FindOne();
        //    return result is not null
        //        ? Mapper.MapFromSearchResult<FindResult>(result)
        //        : null;
        //}
        //public FindResult? FindOne(ISearchOptions oneOffOptions)
        //{
        //    _builder.Clear();
        //    _searcher.Filter = this.Filter?.WriteTo(_builder).ToString();

        //    using (DirectorySearcher oneOffSearcher = new(_searcher.SearchRoot))
        //    {
        //        SearchResult? result = 
        //            SetSearcher(oneOffOptions, new StringBuilder(_builder.Capacity), oneOffSearcher)
        //            .FindOne();

        //        return result is not null 
        //            ? Mapper.MapFromSearchResult<FindResult>(result)
        //            : null;
        //    }
        //}

        //public List<FindResult> FindAll()
        //{
        //    _builder.Clear();
        //    _searcher.Filter = this.Filter?.WriteTo(_builder).ToString();

        //    var list = new List<FindResult>();

        //    using (SearchResultCollection resultCol = _searcher.FindAll())
        //    {
        //        foreach (SearchResult item in resultCol)
        //        {
        //            list.Add(Mapper.MapFromSearchResult<FindResult>(item));
        //        }
        //    }

        //    list.TrimExcess();

        //    return list;
        //}
        public List<FindResult> FindAll(ISearchOptions oneOffOptions, out string ldapFilter, out string? hostContacted)
        {
            ldapFilter = string.Empty;
            hostContacted = null;
            ArgumentNullException.ThrowIfNull(oneOffOptions);

            List<FindResult> list = new();
            using (DirectorySearcher oneOffSearcher = new(_searcher.SearchRoot))
            {
                hostContacted = oneOffSearcher.SearchRoot?.Options?.GetCurrentServerName();

                using (var resultCol = SetSearcher(oneOffOptions, _lazyBuilder.Value, oneOffSearcher)
                                       .FindAll())
                {
                    ldapFilter = _lazyBuilder.Value.ToString();
                    foreach (SearchResult result in resultCol)
                    {
                        FindResult findResult = Mapper.MapFromSearchResult<FindResult>(result, _enumDictionary);
                        list.Add(findResult);
                    }
                }
            }

            list.TrimExcess();
            return list;
        }

        private static DirectorySearcher SetSearcher(ISearchOptions options, StringBuilder builder, DirectorySearcher searcher)
        {
            if (options.Filter is not null)
            {
                searcher.Filter = options.Filter.WriteTo(builder).ToString();
            }
            
            searcher.Sort = options.GetSortOption();
            searcher.SizeLimit = options.SizeLimit;
            searcher.PageSize = options.PageSize;
            searcher.SearchScope = options.SearchScope;
            if (options.PropertiesToLoad is not null)
            {
                searcher.PropertiesToLoad.Clear();
                foreach (string prop in options.PropertiesToLoad.Where(x => !string.IsNullOrWhiteSpace(x)))
                {
                    _ = searcher.PropertiesToLoad.Add(prop);
                }
            }

            return searcher;
        }
    }
}
