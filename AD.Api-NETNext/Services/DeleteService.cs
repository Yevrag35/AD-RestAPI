using AD.Api.Ldap;
using AD.Api.Ldap.Operations;
using System.Security.Claims;

namespace AD.Api.Services
{
    public interface IDeleteService
    {
        ISuccessResult Delete(string distinguishedName, string? domain, ClaimsPrincipal claimsPrincipal);
    }

    internal sealed class DeleteService : IDeleteService
    {
        private IConnectionService Connections { get; }
        private IRestrictionService Restrictions { get; }
        private IResultService Results { get; }

        public DeleteService(IConnectionService connectionService, IRestrictionService restrictionService, IResultService resultService)
        {
            this.Connections = connectionService;
            this.Restrictions = restrictionService;
            this.Results = resultService;
        }

        #region SERVICE METHODS
        /// <summary>
        /// 
        /// </summary>
        /// <param name="distinguishedName"></param>
        /// <param name="claimsPrincipal"></param>
        /// <param name="domain"></param>
        public ISuccessResult Delete(string? distinguishedName, string? domain, ClaimsPrincipal claimsPrincipal)
        {
            if (string.IsNullOrWhiteSpace(distinguishedName))
            {
                return this.Results.GetError(new ArgumentNullException(nameof(distinguishedName)), nameof(distinguishedName));
            }

            using var connection = this.Connections.GetConnection(options =>
            {
                options.Domain = domain;
                options.DontDisposeHandle = false;
                options.Principal = claimsPrincipal;
            });

            using var dirEntry = connection.GetDirectoryEntry(distinguishedName);

            string? objectClass = connection.GetProperty<string>(dirEntry, "objectClass");
            if (!this.Restrictions.IsAllowed(OperationType.Delete, objectClass))
            {
                return new OperationResult
                {
                    Message = $"Not allowed to delete an object of type '{objectClass}' as it's restricted.",
                    Success = false
                };
            }

            if (connection.IsCriticalSystemObject(dirEntry))
            {
                return this.Results.GetError("Cannot delete a critical system object.", OperationType.Delete);
            }

            if (connection.IsProtectedObject(dirEntry))
            {
                return this.Results.GetError("Cannot delete an object that is protected from deletion.", OperationType.Delete);
            }

            try
            {
                connection.DeleteEntry(dirEntry);
                return new OperationResult
                {
                    Success = true,
                    Message = "Successfully deleted the object."
                };
            }
            catch (Exception e)
            {
                return this.Results.GetError($"Unable to delete \"{distinguishedName}\"", OperationType.Delete, e);
            }
        }

        #endregion
    }
}
