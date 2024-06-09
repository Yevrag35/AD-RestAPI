using AD.Api.Ldap;
using AD.Api.Ldap.Operations;
using AD.Api.Schema;
using System.DirectoryServices;

namespace AD.Api.Services
{
    public interface IEditService
    {
        ISuccessResult Edit(EditOperationRequest request);
    }

    public class LdapEditService : OperationServiceBase, IEditService
    {
        private IConnectionService Connections { get; }
        private IRestrictionService Restrictions { get; }
        private ISchemaService Schema { get; }

        public LdapEditService(IConnectionService connectionService, ISchemaService schemaService, IRestrictionService restrictionService)
        {
            this.Connections = connectionService;
            this.Restrictions = restrictionService;
            this.Schema = schemaService;
        }

        public ISuccessResult Edit(EditOperationRequest request)
        {
            ArgumentNullException.ThrowIfNull(request.DistinguishedName);

            if (request.EditOperations.Count <= 0)
            {
                return new OperationResult
                {
                    Message = "No edit operations were specified.",
                    Success = false
                };
            }

            using var connection = this.Connections.GetConnection(options =>
            {
                options.Principal = request.ClaimsPrincipal;
                options.Domain = request.Domain;
                options.DontDisposeHandle = false;
            });

            using var dirEntry = connection.GetDirectoryEntry(request.DistinguishedName);

            string? objectClass = connection.GetSchemaClassName(dirEntry);
            if (!this.Restrictions.IsAllowed(OperationType.Set, objectClass))
            {
                return new OperationResult
                {
                    Message = $"Not allowed to edit an object of type '{objectClass}' as it's restricted",
                    Success = false
                };
            }

            foreach (ILdapOperation operation in request.EditOperations)
            {
                PropertyValueCollection col = connection.GetValueCollection(dirEntry, operation.Property);

                if (this.Schema.Dictionary.TryGetValue(operation.Property, out SchemaProperty? schemaProperty))
                {
                    bool resultPerform = operation.Perform(col, schemaProperty);
                }
            }

            OperationResult result = CommitChanges(dirEntry);

            return result;
        }
    }
}
