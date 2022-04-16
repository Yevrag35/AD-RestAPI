using AD.Api.Extensions;
using AD.Api.Ldap;
using AD.Api.Ldap.Operations;
using AD.Api.Schema;
using Microsoft.Win32.SafeHandles;
using System.DirectoryServices;
using System.Security.Principal;

namespace AD.Api.Services
{
    public interface IEditService
    {
        //ISuccessResult Edit(EditOperationRequest request);
        ISuccessResult Edit(DirectoryEntry dirEntry, List<ILdapOperation> operations, SafeAccessTokenHandle? token);
    }

    public class LdapEditService : OperationServiceBase, IEditService
    {
        //private IConnectionService Connections { get; }
        private ISchemaService Schema { get; }

        public LdapEditService(ISchemaService schemaService)
        {
            this.Schema = schemaService;
        }

        public ISuccessResult Edit(DirectoryEntry dirEntry, List<ILdapOperation> operations, SafeAccessTokenHandle? token)
        {
            if (operations.Count <= 0)
                return new OperationResult
                {
                    Message = "No edit operations were specified.",
                    Success = false
                };

            foreach (ILdapOperation operation in operations)
            {
                PropertyValueCollection col = GetCollection(operation.Property, dirEntry, token);

                if (this.Schema.Dictionary.TryGetValue(operation.Property, out SchemaProperty? schemaProperty))
                {
                    bool resultPerform = operation.Perform(col, schemaProperty);
                }
            }

            OperationResult result = CommitChanges(dirEntry);

            return result;
        }

        private static PropertyValueCollection GetCollection(string propertyName, DirectoryEntry dirEntry, SafeAccessTokenHandle? token)
        {
            if (token is null)
                return GetCollection(propertyName, dirEntry);

            return WindowsIdentity.RunImpersonated(token, () =>
            {
                return dirEntry.Properties.TryGetPropertyValueCollection(propertyName, out PropertyValueCollection? resultCol)
                    ? resultCol
                    : throw new InvalidOperationException($"No property called '{propertyName}' was found.");
            });
        }

        private static PropertyValueCollection GetCollection(string propertyName, DirectoryEntry dirEntry)
        {
            return dirEntry.Properties.TryGetPropertyValueCollection(propertyName, out PropertyValueCollection? col)
                ? col
                : throw new InvalidOperationException($"No property named '{propertyName}' was found.");
        }
    }
}
