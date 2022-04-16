using AD.Api.Extensions;
using AD.Api.Ldap;
using AD.Api.Ldap.Operations;
using AD.Api.Schema;
using System.DirectoryServices;
using System.Security.Principal;

namespace AD.Api.Services
{
    public interface IEditService
    {
        ISuccessResult Edit(EditOperationRequest request);
    }

    public class LdapEditService : OperationServiceBase, IEditService
    {
        private IConnectionService Connections { get; }
        private ISchemaService Schema { get; }

        public LdapEditService(IConnectionService connectionService, ISchemaService schemaService)
        {
            this.Connections = connectionService;
            this.Schema = schemaService;
        }

        public ISuccessResult Edit(EditOperationRequest request)
        {
            if (request.EditOperations.Count <= 0)
                return new OperationResult
                {
                    Message = "No edit operations were specified.",
                    Success = false
                };

            using (LdapConnection connection = this.Connections.GetConnection(new ConnectionOptions
            {
                Domain = request.Domain,
                Principal = request.ClaimsPrincipal
            }))
            {
                if (!this.Schema.HasAllAttributesCached(request.EditOperations.Select(x => x.Property), out List<string>? missing))
                        this.Schema.LoadAttributes(missing, connection);

                using (var dirEntry = connection.GetDirectoryEntry(request.DistinguishedName))
                {
                    foreach (ILdapOperation operation in request.EditOperations)
                    {
                        if (dirEntry.Properties.TryGetPropertyValueCollection(operation.Property, out PropertyValueCollection? collection)
                            &&
                            this.Schema.Dictionary.TryGetValue(operation.Property, out SchemaProperty? schemaProperty)
                            &&
                            !operation.Perform(collection, schemaProperty))
                        {
                            return new OperationResult
                            {
                                Error = new ErrorDetails
                                {
                                    OperationType = operation.OperationType,
                                    Property = operation.Property,
                                },
                                Message = "Unable to perform the requested operation.",
                                Success = false
                            };
                        }
                    }

                    return CommitChanges(dirEntry);
                }
            }
        }
    }
}
