using AD.Api.Extensions;
using AD.Api.Ldap;
using AD.Api.Ldap.Models;
using AD.Api.Ldap.Operations;
using AD.Api.Ldap.Path;
using AD.Api.Schema;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace AD.Api.Services
{
    public interface ICreateService
    {
        ISuccessResult Create(CreateOperationRequest request);
        //OperationResult CreateOnBehalfOf(CreateOperationRequest request, WindowsIdentity windowsIdentity);
    }

    public class LdapCreateService : OperationServiceBase, ICreateService
    {
        private IConnectionService Connections { get; }
        private IPasswordService Passwords { get; }
        private IResultService Results { get; }
        private ISchemaService Schema { get; }

        public LdapCreateService(IConnectionService connectionService, ISchemaService schemaService,
            IPasswordService passwordService, IResultService resultService)
        {
            this.Connections = connectionService;
            this.Passwords = passwordService;
            this.Results = resultService;
            this.Schema = schemaService;
        }

        //public OperationResult CreateOnBehalfOf(CreateOperationRequest request, WindowsIdentity windowsIdentity)
        //{
        //    return WindowsIdentity.RunImpersonated(windowsIdentity.AccessToken, () =>
        //    {
        //        return this.CreateInContext(request);
        //    });
        //}

        public ISuccessResult Create(CreateOperationRequest request)
        {
            using var connection = this.Connections.GetConnection(new ConnectionOptions
            {
                Domain = request.Domain,
                DontDisposeHandle = false,
                Principal = request.ClaimsPrincipal
            });
            using var pathEntry = GetDirectoryEntryFromRequest(connection, request);

            //CommonName cn = request.CommonName;

            DirectoryEntry createdEntry;
            OperationResult result;
            try
            {
                createdEntry = connection.AddChildEntry(request.CommonName, pathEntry, request.Type);
                result = CommitChanges(createdEntry, true);
                result.DistinguishedName = createdEntry.Properties.GetFirstValue<string>(nameof(result.DistinguishedName));
            }
            catch (Exception e)
            {
                connection.Dispose();
                pathEntry.Dispose();
                return new OperationResult
                {
                    Error = new ErrorDetails
                    {
                        ErrorCode = e.HResult,
                        ExtendedMessage = e.GetBaseException().Message,
                        OperationType = OperationType.Commit
                    },
                    Message = e.Message,
                    Success = false
                };
            }

            bool hasSetPass = false;
            if (request is UserCreateOperationRequest userRequest &&
                !this.TrySetPassword(userRequest, result, createdEntry, out OperationResult? passwordResult)
                && passwordResult is not null)
            {
                createdEntry.Dispose();
                return passwordResult;
            }
            else
                hasSetPass = true;

            if (request is GroupCreateOperationRequest groupRequest
                && 
                !this.TrySetDomainLocal(groupRequest, result, connection, createdEntry, out ISuccessResult? groupChangeResult)
                &&
                groupChangeResult is not null)
            {
                return groupChangeResult;
            }
            else if (request is OUCreateOperationRequest ouRequest
                && !this.TrySetProtected(ouRequest, result, createdEntry, out ISuccessResult? ouResult))
            {
                return ouResult;
            }

            if (result.Success && request.Properties.Count > 0)
            {
                foreach (var kvp in request.Properties.Where(x => x.Value is not null))
                {
                    if (createdEntry.Properties.TryGetPropertyValueCollection(kvp.Key, out PropertyValueCollection? collection))
                    {
                        // Let's figure out Schema Caching
                        if (!this.Schema.Dictionary.TryGetValue(kvp.Key, out SchemaProperty? schemaProperty))
                            throw new InvalidOperationException("Schema not loaded.");

                        switch (kvp.Value.Type)
                        {
                            case JTokenType.Object:
                            case JTokenType.None:
                            case JTokenType.Null:
                            case JTokenType.Comment:
                            case JTokenType.Constructor:
                            case JTokenType.Undefined:
                            case JTokenType.Property:
                                break;

                            case JTokenType.Array:
                            {
                                Action<PropertyValueCollection, object[]> action = GetMultiValueAction(schemaProperty);

                                object[]? arr = kvp.Value.ToObject<object[]>();
                                if (arr is not null)
                                    action(collection, arr);

                                break;
                            }

                            default:
                            {
                                if (schemaProperty.Name.Equals(nameof(UserAccountControl), StringComparison.CurrentCultureIgnoreCase)
                                    &&
                                    hasSetPass
                                    &&
                                    request is UserCreateOperationRequest userReq
                                    &&
                                    userReq.UserAccountControl.HasValue
                                    &&
                                    userReq.UserAccountControl.Value.HasFlag(UserAccountControl.PasswordNotRequired))
                                {
                                    UserAccountControl uac = userReq.UserAccountControl.Value;
                                    uac &= ~UserAccountControl.PasswordNotRequired;
                                    collection.Value = (int)uac;
                                    break;
                                }

                                Action<PropertyValueCollection, object> action = GetSingleValueAction(schemaProperty);

                                object? obj = kvp.Value.ToObject<object>();
                                if (obj is not null)
                                    action(collection, obj);

                                break;
                            }
                        }
                    }
                }

                result = CommitChanges(createdEntry);
                result.DistinguishedName = createdEntry.Properties.GetFirstValue<string>(nameof(result.DistinguishedName));
            }

            createdEntry.Dispose();

            return result;
        }

        private static DirectoryEntry GetDirectoryEntryFromRequest(LdapConnection connection, CreateOperationRequest request)
        {
            return string.IsNullOrWhiteSpace(request.Path)
                ? connection.GetDirectoryEntry(connection.GetWellKnownPath(FromCreationType(request.Type)))
                : connection.GetDirectoryEntry(request.Path);
        }

        private static WellKnownObjectValue FromCreationType(CreationType type)
        {
            return type switch
            {
                CreationType.Computer => WellKnownObjectValue.Computers,
                _ => WellKnownObjectValue.Users,
            };
        }

        private static Action<PropertyValueCollection, object[]> GetMultiValueAction(SchemaProperty schemaProperty)
        {
            if (schemaProperty.IsSingleValued)
                throw new InvalidOperationException($"'{schemaProperty.Name}' does not allow for multiple values.");

            return (pvc, values) =>
            {
                pvc.Clear();
                pvc.AddRange(values);
            };
        }

        private static Action<PropertyValueCollection, object> GetSingleValueAction(SchemaProperty schemaProperty)
        {
            if (!schemaProperty.IsSingleValued)
                return (pvc, value) => pvc.Add(value);

            if (schemaProperty.HasRange)
            {
                return (pvc, value) =>
                {
                    int count = 0;
                    if (value is int intVal)
                        count = intVal;

                    else if (value is string strVal)
                        count = strVal.Length;

                    if ((schemaProperty.RangeLower.HasValue && 
                        count.CompareTo(schemaProperty.RangeLower.Value) < 0)
                        ||
                        (schemaProperty.RangeUpper.HasValue &&
                        count.CompareTo(schemaProperty.RangeUpper) > 0))
                    {
                        throw new ArgumentOutOfRangeException($"{value} is outside of the range allowed by the attribute.");
                    }

                    pvc.Value = value;
                };
            }
            else
            {
                return (pvc, value) => pvc.Value = value;
            }
        }

        private bool TrySetPassword(UserCreateOperationRequest request, OperationResult creationResult, DirectoryEntry createdEntry, 
            [MaybeNullWhen(false)] out OperationResult? passwordResult)
        {
            passwordResult = null;
            if (creationResult.Success && this.Passwords.TryGetFromBase64(request, out byte[]? passBytes))
            {
                var passResult = this.Passwords.SetPassword(createdEntry, passBytes);
                if (passResult.Success)
                {
                    creationResult.Message = $"{creationResult.Message}; {passResult.Message}";
                    return true;
                }
                else
                {
                    passwordResult = passResult;
                }
            }

            return false;
        }

        private bool TrySetDomainLocal(GroupCreateOperationRequest request, OperationResult creationResult,
            LdapConnection connection, DirectoryEntry groupEntry, out ISuccessResult? groupChangeResult)
        {
            groupChangeResult = null;
            if (!creationResult.Success)
                return false;

            if (!request.GroupTypeValue.HasFlag(GroupType.DomainLocal))
                return true;

            // We have to change the group to Universal first.
            GroupType changeTo = request.GroupTypeValue;
            changeTo &= ~GroupType.DomainLocal;
            changeTo |= GroupType.Universal;

            if (!groupEntry.Properties.TryGetPropertyValueCollection(nameof(request.GroupType), out PropertyValueCollection? propValCol))
                return false;

            try
            {
                propValCol.Value = unchecked((int)changeTo);
                groupEntry.CommitChanges();
                groupEntry.RefreshCache();

                changeTo &= ~GroupType.Universal;
                changeTo |= GroupType.DomainLocal;

                propValCol.Value = unchecked((int)changeTo);
                groupEntry.CommitChanges();
                groupEntry.RefreshCache();

                return request.Properties.Remove(nameof(GroupType));
            }
            catch (Exception ex)
            {
                groupChangeResult = this.Results.GetError(ex, nameof(request.GroupType));
                return false;
            }
        }

        private bool TrySetProtected(OUCreateOperationRequest request, OperationResult creationResult,
            DirectoryEntry ouEntry, [NotNullWhen(false)] out ISuccessResult? ouResult)
        {
            ouResult = null;

            if (!request.ProtectFromAccidentalDeletion)
                return true;

            ActiveDirectoryAccessRule rule = new(
                new NTAccount("Everyone"),
                ActiveDirectoryRights.DeleteTree | ActiveDirectoryRights.Delete,
                AccessControlType.Deny
            );

            ouEntry.ObjectSecurity.AddAccessRule(rule);

            try
            {
                ouEntry.CommitChanges();
                ouEntry.RefreshCache();

                return true;
            } 
            catch (Exception ex)
            {
                ouResult = this.Results.GetError(ex, "object security");
                return false;
            }
        }
    }
}
