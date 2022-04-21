using AD.Api.Extensions;
using AD.Api.Ldap.Attributes;
using AD.Api.Ldap.Connection;
using AD.Api.Ldap.Filters;
using AD.Api.Ldap.Models;
using AD.Api.Ldap.Operations;
using AD.Api.Ldap.Path;
using AD.Api.Ldap.Search;
using AD.Api.Schema;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.DirectoryServices.ActiveDirectory;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Security.Principal;

namespace AD.Api.Ldap
{
    public sealed class LdapConnection : IDisposable
    {
        private SafeAccessTokenHandle? _accessToken;
        private readonly NetworkCredential? _creds;
        private bool _disposed;
        private readonly bool _dontDisposeToken;
        private readonly ILdapEnumDictionary _enumDictionary;

        public AuthenticationTypes? AuthenticationTypes { get; }
        public string? Host { get; }
        public bool IsForestRoot { get; }
        public Protocol Protocol { get; }
        public PathValue RootDSE { get; }
        public PathValue SearchBase { get; }
        public bool UseSchemaCache { get; }
        public bool UseSSL { get; }

        public LdapConnection(ILdapConnectionOptions options, ILdapEnumDictionary enumDictionary)
        {
            _accessToken = options.Token;
            _dontDisposeToken = options.DontDisposeToken;
            _enumDictionary = enumDictionary;

            this.AuthenticationTypes = options.AuthenticationTypes;
            this.Host = options.Host;
            this.IsForestRoot = options.IsForest;
            this.Protocol = options.Protocol;
            this.UseSchemaCache = options.UseSchemaCache;
            this.UseSSL = options.UseSSL;
            _creds = options.GetCredential();

            this.RootDSE = new PathValue(this.Protocol)
            {
                DistinguishedName = "RootDSE",
                Host = this.Host,
                UseSsl = this.UseSSL
            };

            this.SearchBase = new PathValue(this.Protocol)
            {
                DistinguishedName = options.DistinguishedName,
                Host = this.Host,
                UseSsl = this.UseSSL
            };
        }

        #region GET DIRECTORY ENTRIES

        public bool ChildEntryExists(CommonName cn, DirectoryEntry parent)
        {
            return ExecuteInContext(_accessToken, () =>
            {
                try
                {
                    var de = parent.Children.Find(cn.Value);
                    de.Dispose();
                    return true;
                }
                catch (DirectoryServicesCOMException comEx)
                {
                    return comEx.Message.Contains("There is no such object on the server", StringComparison.CurrentCultureIgnoreCase)
                        ? false
                        : throw comEx;
                }
            });
        }

        /// <summary>
        /// Constructs a <see cref="DirectoryEntry"/> from the specified DistinguishedName (dn).
        /// </summary>
        /// <param name="dn">The distinguished name of the <see cref="DirectoryEntry"/>.</param>
        /// <returns>
        ///     A new <see cref="DirectoryEntry"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="dn"/> is <see langword="null"/>.</exception>
        public DirectoryEntry GetDirectoryEntry(string dn)
        {
            if (string.IsNullOrWhiteSpace(dn))
                throw new ArgumentNullException(nameof(dn));

            return this.GetDirectoryEntry(new PathValue(this.Protocol)
            {
                DistinguishedName = dn,
                Host = this.Host,
                UseSsl = this.UseSSL
            });
        }
        /// <summary>
        /// Constructs a <see cref="DirectoryEntry"/> from the specified <see cref="PathValue"/>.
        /// </summary>
        /// <param name="path">The path to use constructing the <see cref="DirectoryEntry"/>.</param>
        /// <returns>
        ///     A new <see cref="DirectoryEntry"/> built from the specified path.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="path"/> is <see langword="null"/>.</exception>
        public DirectoryEntry GetDirectoryEntry(PathValue path)
        {
            if (path is null)
                throw new ArgumentNullException(nameof(path));

            return CreateEntry(path, _creds, this.AuthenticationTypes, _accessToken);
        }
        /// <summary>
        /// Constructs a <see cref="DirectoryEntry"/> from the specified <see cref="IPathed"/> implementation. 
        /// </summary>
        /// <param name="pathedObject">The implementation that provides a <see cref="PathValue"/>.</param>
        /// <returns>
        ///     A new <see cref="DirectoryEntry"/>, or
        ///     <see langword="null"/> if <paramref name="pathedObject"/> or <see cref="IPathed.Path"/> is <see langword="null"/>.
        /// </returns>
        public DirectoryEntry? GetDirectoryEntry(IPathed? pathedObject)
        {
            return pathedObject is not null && pathedObject.Path is not null
                ? this.GetDirectoryEntry(pathedObject.Path)
                : null;
        }

        /// <summary>
        /// Constructs a <see cref="DirectoryEntry"/> from the connection's specified RootDSE.
        /// </summary>
        /// <returns>
        ///     A <see cref="DirectoryEntry"/> pointing to LDAP://RootDSE
        /// </returns>
        public DirectoryEntry GetRootDSE()
        {
            return this.GetDirectoryEntry(this.RootDSE);
        }

        /// <summary>
        /// Constructs a <see cref="DirectoryEntry"/> from the connection's set search base DN.
        /// </summary>
        /// <returns>
        ///     A new <see cref="DirectoryEntry"/> pointing to the <see cref="LdapConnection.SearchBase"/>.
        /// </returns>
        public DirectoryEntry GetSearchBase()
        {
            return this.GetDirectoryEntry(this.SearchBase);
        }

        #endregion
        public DirectoryEntry AddChildEntry(string commonName, DirectoryEntry parent, CreationType creationType)
        {
            if (string.IsNullOrWhiteSpace(commonName) || commonName.Equals(Strings.CN_Prefix, StringComparison.CurrentCultureIgnoreCase))
                throw new ArgumentNullException(nameof(commonName));

            CommonName cn = CommonName.Create(commonName, creationType == CreationType.OrganizationalUnit);

            return ExecuteInContext(_accessToken, () =>
            {
                return parent.Children.Add(cn.Value, creationType.ToString().ToLower());
            });
        }

        [Obsolete("Not needed")]
        public OperationResult CommitChanges(DirectoryEntry directoryEntry, bool andRefresh = false)
        {
            return ExecuteInContext(_accessToken, () =>
            {
                try
                {
                    directoryEntry.CommitChanges();
                    if (andRefresh)
                        directoryEntry.RefreshCache();

                    return new OperationResult
                    {
                        Message = "Successfully updated entry.",
                        Success = true
                    };
                }
                catch (DirectoryServicesCOMException comException)
                {
                    return new OperationResult
                    {
                        Message = comException.Message,
                        Success = false,
                        Error = new ErrorDetails(comException)
                        {
                            OperationType = OperationType.Commit
                        }
                    };
                }
                catch (Exception genericException)
                {
                    string? extMsg = null;
                    Exception baseEx = genericException.GetBaseException();
                    if (!ReferenceEquals(genericException, baseEx))
                        extMsg = baseEx.Message;

                    return new OperationResult
                    {
                        Message = genericException.Message,
                        Success = false,
                        Error = new ErrorDetails
                        {
                            ErrorCode = genericException.HResult,
                            ExtendedMessage = extMsg,
                            OperationType = OperationType.Commit
                        }
                    };
                }
            });
        }

        public void DeleteEntry(DirectoryEntry entryToDelete)
        {
            ExecuteInContext(_accessToken, () => entryToDelete.DeleteTree());
        }

        [Obsolete("Not needed")]
        public bool DoEditOperation(ILdapOperation operation, PropertyValueCollection collection, SchemaProperty schemaProperty)
        {
            return ExecuteInContext(_accessToken, () => operation.Perform(collection, schemaProperty));
        }

        public string? RenameEntry(DirectoryEntry entryToRename, CommonName commonName)
        {
            ArgumentNullException.ThrowIfNull(entryToRename);
            ArgumentNullException.ThrowIfNull(commonName);

            return ExecuteInContext(_accessToken, () =>
            {
                entryToRename.Rename(commonName.Value);
                return entryToRename.Properties[Strings.DistinguishedName].Value as string;
            });
        }

        #region CREATE SEARCHERS
        //[Obsolete]
        //public ILdapSearcher CreateSearcher(PathValue searchBase, IFilterStatement? filter = null)
        //{
        //    return new Searcher(this.GetDirectoryEntry(searchBase))
        //    {
        //        Filter = filter
        //    };
        //}

        public ILdapSearcher CreateSearcher(ISearchOptions? options = null)
        {
            return new Searcher(this, _enumDictionary, options);
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public DirectoryContext GetForestContext()
        {
            if (!this.IsForestRoot)
                throw new InvalidOperationException($"Cannot get AD forest context when the connection was not specified as the Forest Root.");

            string host;
            if (string.IsNullOrWhiteSpace(this.Host))
            {
                using DirectoryEntry rootDse = this.GetRootDSE();
                host = GetForestName(rootDse);
            }
            else
                host = this.Host;

            return _creds is null
                ? new DirectoryContext(DirectoryContextType.Forest, host)
                : new DirectoryContext(DirectoryContextType.Forest, host, _creds.UserName, _creds.Password);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public PathValue GetWellKnownPath(WellKnownObjectValue value)
        {
            using DirectoryEntry rootDse = this.GetRootDSE();

            if (!rootDse.Properties.TryGetFirstValue(Strings.DefaultNamingContext, out string? namingContext))
            {
                rootDse.Dispose();
                throw new InvalidOperationException();  // Figure out what to do later.
            }

            WellKnownObject wko = WellKnownObject.Create(value, namingContext);

            var wkoPath = new PathValue(this.Protocol)
            {
                DistinguishedName = wko,
                Host = this.Host,
                UseSsl = this.UseSSL
            };

            using (var wkoDe = this.GetDirectoryEntry(wkoPath))
            {
                if (wkoDe.Properties.TryGetFirstValue(nameof(wkoPath.DistinguishedName), out string? realDn))
                {
                    return new PathValue(this.Protocol)
                    {
                        DistinguishedName = realDn,
                        Host = this.Host,
                        UseSsl = this.UseSSL
                    };
                }
                else
                    return wkoPath; // which will error...
            }
        }

        public T? GetProperty<T>(DirectoryEntry directoryEntry, string? propertyName)
        {
            if (string.IsNullOrWhiteSpace(propertyName))
                throw new ArgumentNullException(nameof(propertyName));

            return ExecuteInContext(_accessToken, () =>
            {
                if (!directoryEntry.Properties.TryGetFirstValue(propertyName, out T? value))
                    value = default;

                return value;
            });
        }

        public bool IsCriticalSystemObject(DirectoryEntry directoryEntry)
        {
            return ExecuteInContext(_accessToken, () =>
            {
                return directoryEntry.Properties.Contains(Strings.CriticalSystemObject)
                       &&
                       directoryEntry.Properties[Strings.CriticalSystemObject].Value is bool trueOrFalse
                       &&
                       trueOrFalse;
            });
        }

        public bool IsProtectedObject(DirectoryEntry directoryEntry)
        {
            return ExecuteInContext(_accessToken, () =>
            {
                var ruleCollection = directoryEntry.ObjectSecurity.GetAccessRules(true, true, typeof(NTAccount));
                NTAccount everyone = new(Strings.NTAccount_Everyone);
                return ruleCollection.Cast<ActiveDirectoryAccessRule>()
                    .Any(rule =>
                        rule.AccessControlType == AccessControlType.Deny
                        &&
                        rule.IdentityReference.Equals(everyone)
                        &&
                        (
                            rule.ActiveDirectoryRights.HasFlag(ActiveDirectoryRights.Delete)
                            ||
                            rule.ActiveDirectoryRights.HasFlag(ActiveDirectoryRights.DeleteTree)
                        )
                    );
            });
        }

        public void MoveObject(DirectoryEntry entryToMove, DirectoryEntry destination)
        {
            ExecuteInContext(_accessToken, () =>
            {
                entryToMove.MoveTo(destination);
            });
        }

        private static DirectoryEntry CreateEntry(PathValue path, NetworkCredential? creds, AuthenticationTypes? authenticationTypes, SafeAccessTokenHandle? token)
        {
            Func<DirectoryEntry> createFunc;

            if (creds is null)
                createFunc = () => new DirectoryEntry(path.GetValue());

            else
            {
                createFunc = !authenticationTypes.HasValue
                    ? () => new DirectoryEntry(path.GetValue(), creds.UserName, creds.Password)
                    : () => new DirectoryEntry(path.GetValue(), creds.UserName, creds.Password, authenticationTypes.Value);
            }

            return ExecuteInContext(token, createFunc);
        }

        private static void ExecuteInContext(SafeAccessTokenHandle? token, Action action)
        {
            if (token is not null)
                WindowsIdentity.RunImpersonated(token, action);

            else
                action();
        }
        private static T ExecuteInContext<T>(SafeAccessTokenHandle? token, Func<T> function)
        {
            return token is not null
                ? WindowsIdentity.RunImpersonated(token, function)
                : function();
        }

        /// <summary>
        /// Retrieves the DNS forest name from the RootDSE.
        /// </summary>
        /// <param name="rootDse"></param>
        /// <returns>The DNS Host name from the RootDSE.</returns>
        /// <exception cref="InvalidOperationException">Couldn't find "dnsHostName" in <paramref name="rootDse"/>.</exception>
        private static string GetForestName(DirectoryEntry rootDse)
        {
            if (!rootDse.Properties.TryGetFirstValue("dnsHostName", out string? hostName))
                throw new InvalidOperationException($"Unable to get the connection name from RootDSE");

            return hostName;
        }

        private static string TruncateCN(string commonName)
        {
            if (!commonName.StartsWith(Strings.CN_Prefix, StringComparison.CurrentCultureIgnoreCase))
                return commonName;

            (string cn, int startAt) tuple = (commonName, Strings.CN_Prefix.Length);

            return string.Create(commonName.Length - tuple.startAt, tuple, (chars, state) =>
            {
                state.cn
                    .AsSpan()
                    .Slice(state.startAt)
                    .CopyTo(chars);
            });
        }

        #region IDISPOSABLE IMPLEMENTATION
        public void Dispose()
        {
            if (_disposed)
                return;

            if (!_dontDisposeToken)
                _accessToken?.Dispose();

            else
                _accessToken = null;

            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
