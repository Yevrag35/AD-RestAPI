using AD.Api.Ldap;
using AD.Api.Ldap.Models;
using AD.Api.Ldap.Operations;
using System.DirectoryServices;

namespace AD.Api.Services
{
    public interface IMoveService
    {
        ISuccessResult MoveObject(MoveRequest moveRequest);
    }

    public class MoveService : IMoveService
    {
        private IConnectionService Connections { get; }
        private IResultService Results { get; }
        
        public MoveService(IConnectionService connectionService, IResultService resultService)
        {
            this.Connections = connectionService;
            this.Results = resultService;
        }

        public ISuccessResult MoveObject(MoveRequest moveRequest)
        {
            if (!moveRequest.IsValid)
                return this.Results.GetError("The destination cannot be the same as the source.", OperationType.Move, "dest");

            using var connection = this.Connections.GetConnection(new ConnectionOptions
            {
                Domain = moveRequest.Domain,
                DontDisposeHandle = true,
                Principal = moveRequest.ClaimsPrincipal
            });

            using DirectoryEntry entryToMove = connection.GetDirectoryEntry(moveRequest.DistinguishedName);
            if (connection.IsCriticalSystemObject(entryToMove))
                return this.Results.GetError("Cannot move a critical system object.", OperationType.Move, "dn");

            else if (connection.IsProtectedObject(entryToMove))
                return this.Results.GetError("Cannot move an object protected from accidental deletion.", OperationType.Move, "dn");

            using DirectoryEntry destination = connection.GetDirectoryEntry(moveRequest.DestinationPath);

            try
            {
                connection.MoveObject(entryToMove, destination);
            }
            catch (Exception e)
            {
                return this.Results.GetError("Unable to move object", OperationType.Move, e);
            }

            string? newDn = connection.GetProperty<string>(entryToMove, Strings.DistinguishedName);
            return new OperationResult
            {
                Success = true,
                DistinguishedName = newDn,
                Message = "Successfully moved the object."
            };
        }
    }
}
