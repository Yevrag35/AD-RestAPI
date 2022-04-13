using AD.Api.Extensions;
using AD.Api.Ldap;
using AD.Api.Ldap.Operations;
using System.DirectoryServices;

namespace AD.Api.Services
{
    public interface IEditService
    {
        OperationResult Edit(EditOperationRequest request);
    }

    public class LdapEditService : OperationServiceBase, IEditService
    {
        private IConnectionService Connections { get; }

        public LdapEditService(IConnectionService connectionService)
        {
            this.Connections = connectionService;
        }

        public OperationResult Edit(EditOperationRequest request)
        {
            using (var connection = this.Connections.GetConnection(request.Domain))
            {
                using (var dirEntry = connection.GetDirectoryEntry(request.DistinguishedName))
                {
                    foreach (ILdapOperation operation in request.EditOperations)
                    {
                        if (dirEntry.Properties.TryGetPropertyValueCollection(operation.Property, out PropertyValueCollection? collection)
                            &&
                            !operation.Perform(collection))
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
