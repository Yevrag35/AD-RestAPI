using System.DirectoryServices;

namespace AD.Api.Services
{
    public abstract class OperationServiceBase
    {
        protected static OperationResult CommitChanges(DirectoryEntry entry, bool andRefresh = false)
        {
            try
            {
                entry.CommitChanges();
                if (andRefresh)
                {
                    entry.RefreshCache();
                }

                return new OperationResult
                {
                    Message = "Successfully updated entry.",
                    Success = true
                };
            }
            catch (DirectoryServicesCOMException comException)
            {
                return new OperationResult
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
                {
                    extMsg = baseEx.Message;
                }

                return new OperationResult
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
