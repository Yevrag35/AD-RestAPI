using AD.Api.Extensions;
using AD.Api.Ldap;
using AD.Api.Ldap.Operations;
using System.DirectoryServices;

namespace AD.Api.Services
{
    public interface IEditService
    {
        EditResult Edit(OperationRequest request);
    }

    public class LdapEditService : IEditService
    {
        private IConnectionService Connections { get; }

        public LdapEditService(IConnectionService connectionService)
        {
            this.Connections = connectionService;
        }

        public EditResult Edit(OperationRequest request)
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
                            return new EditResult
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

        private static EditResult CommitChanges(DirectoryEntry entry)
        {
            try
            {
                entry.CommitChanges();
                return new EditResult
                {
                    Message = "Successfully updated entry.",
                    Success = true
                };
            }
            catch (DirectoryServicesCOMException comException)
            {
                return new EditResult
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

                return new EditResult
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
        }
    }
}
