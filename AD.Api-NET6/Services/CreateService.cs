using AD.Api.Extensions;
using AD.Api.Ldap;
using AD.Api.Ldap.Models;
using AD.Api.Ldap.Operations;
using AD.Api.Ldap.Path;
using AD.Api.Schema;
using Newtonsoft.Json.Linq;
using System.DirectoryServices;

namespace AD.Api.Services
{
    public interface ICreateService
    {
        OperationResult Create(CreateOperationRequest request);
    }

    public class LdapCreateService : OperationServiceBase, ICreateService
    {
        private IConnectionService Connections { get; }
        private ISchemaService Schema { get; }

        public LdapCreateService(IConnectionService connectionService, ISchemaService schemaService)
        {
            this.Connections = connectionService;
            this.Schema = schemaService;
        }

        public OperationResult Create(CreateOperationRequest request)
        {
            //string classType = request.Type.ToString();
            //if (!this.Schema.IsClassLoaded(classType))
            //{
            //    this.Schema.LoadClass(classType, request.Domain);
            //}

            using var connection = this.Connections.GetConnection(request.Domain);
            using var pathEntry = GetDirectoryEntryFromRequest(connection, request);

            CommonName cn = request.CommonName;

            DirectoryEntry createdEntry;
            OperationResult result;
            try
            {
                createdEntry = pathEntry.Children.Add(cn.Value, request.Type.ToString().ToLower());
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
            
            if (result.Success)
            {
                foreach (var kvp in request.Properties.Where(x => x.Value is not null))
                {
                    if (createdEntry.Properties.TryGetPropertyValueCollection(kvp.Key, out PropertyValueCollection? collection))
                    {
                        // Let's figure out Schema Caching
                        //if (!this.Schema.TryGet(kvp.Key, out SchemaProperty? schemaProperty))
                        //    throw new InvalidOperationException("Schema not laoded.");

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
                                //Action<PropertyValueCollection, object[]> action = GetMultiValueAction(schemaProperty);

                                object[]? arr = kvp.Value.ToObject<object[]>();
                                if (arr is not null)
                                    collection.AddRange(arr);

                                break;
                            }

                            default:
                            {
                                //Action<PropertyValueCollection, object> action = GetSingleValueAction(schemaProperty);

                                object? obj = kvp.Value.ToObject<object>();
                                if (obj is not null)
                                    collection.Value = obj;
                                    //action(collection, obj);

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
                    if (value is int intVal && schemaProperty.HasRange && 
                        (intVal.CompareTo(schemaProperty.RangeLower) < 0
                        ||
                        intVal.CompareTo(schemaProperty.RangeUpper) > 0))
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
    }
}
