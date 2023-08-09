using AD.Api.Ldap;
using AD.Api.Ldap.Models;
using AD.Api.Ldap.Operations;
using AD.Api.Ldap.Path;

namespace AD.Api.Services
{
    public interface IRenameService
    {
        ISuccessResult Rename(RenameRequest request);
    }

    public class RenameService : IRenameService
    {
        private IConnectionService Connections { get; }
        private IRestrictionService Restrictions { get; }
        private IResultService Results { get; }

        public RenameService(IConnectionService connectionService, IRestrictionService restrictionService, IResultService resultService)
        {
            this.Connections = connectionService;
            this.Restrictions = restrictionService;
            this.Results = resultService;
        }

        public ISuccessResult Rename(RenameRequest request)
        {
            using var connection = this.Connections.GetConnection(options =>
            {
                options.Domain = request.Domain;
                options.DontDisposeHandle = false;
                options.Principal = request.ClaimsPrincipal;
            });

            using var dirEntry = connection.GetDirectoryEntry(request.DistinguishedName);

            string? objectClass = connection.GetProperty<string>(dirEntry, "objectClass");
            if (!this.Restrictions.IsAllowed(OperationType.Rename, objectClass))
            {
                return new OperationResult
                {
                    Message = $"Not allowed to rename an object of type '{objectClass}' as it's restricted.",
                    Success = false
                };
            }

            CommonName cn = CommonName.Create(
                request.NewName, dirEntry.SchemaClassName.Equals("organizationalUnit")
            );

            try
            {
                string? newDn = connection.RenameEntry(dirEntry, cn);
                return new OperationResult
                {
                    DistinguishedName = newDn,
                    Message = "Object has been successfully renamed.",
                    Success = true
                };
            }
            catch (Exception e)
            {
                return this.Results.GetError(e, "cn");
            }
        }
    }
}
