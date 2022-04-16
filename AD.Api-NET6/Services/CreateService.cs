using AD.Api.Extensions;
using AD.Api.Ldap;
using AD.Api.Ldap.Models;
using AD.Api.Ldap.Operations;
using AD.Api.Ldap.Path;
using AD.Api.Schema;
using Newtonsoft.Json.Linq;
using System.Diagnostics.CodeAnalysis;
using System.DirectoryServices;
using System.Security.Principal;

namespace AD.Api.Services
{
    public interface ICreateService
    {
        OperationResult Create(CreateOperationRequest request);
        //OperationResult CreateOnBehalfOf(CreateOperationRequest request, WindowsIdentity windowsIdentity);
    }

    public class LdapCreateService : OperationServiceBase, ICreateService
    {
        private IConnectionService Connections { get; }
        private IPasswordService Passwords { get; }
        private ISchemaService Schema { get; }

        public LdapCreateService(IConnectionService connectionService, ISchemaService schemaService,
            IPasswordService passwordService)
        {
            this.Connections = connectionService;
            this.Passwords = passwordService;
            this.Schema = schemaService;
        }

        //public OperationResult CreateOnBehalfOf(CreateOperationRequest request, WindowsIdentity windowsIdentity)
        //{
        //    return WindowsIdentity.RunImpersonated(windowsIdentity.AccessToken, () =>
        //    {
        //        return this.CreateInContext(request);
        //    });
        //}

        public OperationResult Create(CreateOperationRequest request)
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
                //createdEntry = pathEntry.Children.Add(cn.Value, request.Type.ToString().ToLower());
                createdEntry = connection.AddChildEntry(request.CommonName, pathEntry, request.Type);
                result = CommitChanges(createdEntry, true);
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

            if (result.Success)
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
                                    //collection.Value = obj;
                                    action(collection, obj);

                                break;
                            }
                        }
                    }
                }

                result = CommitChanges(createdEntry);
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

        private Action<PropertyValueCollection, object[]> GetMultiValueAction(SchemaProperty schemaProperty)
        {
            if (schemaProperty.IsSingleValued)
                throw new InvalidOperationException($"'{schemaProperty.Name}' does not allow for multiple values.");

            return (pvc, values) =>
            {
                pvc.Clear();
                pvc.AddRange(values);
            };
        }

        private Action<PropertyValueCollection, object> GetSingleValueAction(SchemaProperty schemaProperty)
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
    }
}
