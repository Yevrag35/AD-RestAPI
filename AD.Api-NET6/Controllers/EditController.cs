using AD.Api.Extensions;
using AD.Api.Ldap.Operations;
using AD.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.DirectoryServices;

namespace AD.Api.Controllers
{
    [ApiController]
    [Produces("application/json")]
    [Route("{controller}")]
    public class EditController : ADControllerBase
    {
        public EditController(IConnectionService connectionService, ISerializationService serializationService)
            : base(connectionService, serializationService)
        {
        }

#if DEBUG
        [HttpPost]
        public IActionResult TestSendBack([FromBody] OperationRequest request)
        {
            return Ok(request);
        }
#endif

        [HttpPut]
        public IActionResult PerformEdit([FromBody] OperationRequest request)
        {
            IActionResult response;
            using (var connection = this.Connections.GetConnection())
            {
                using (var de = connection.GetDirectoryEntry(request.DistinguishedName))
                {
                    for (int i = 0; i < request.EditOperations.Count; i++)
                    {
                        ILdapOperation operation = request.EditOperations[i];

                        if (de.Properties.TryGetPropertyValueCollection(operation.Property, out PropertyValueCollection? collection)
                                && !operation.Perform(collection))
                        {
                            return BadRequest(new
                            {
                                Message = "Unable to perform the requested operation.",
                                operation.Property,
                                operation.OperationType
                            });
                        }

                        
                    }

                    try
                    {
                        de.CommitChanges();
                        response = NoContent();
                    }
                    catch (DirectoryServicesCOMException e)
                    {
                        response = base.UnprocessableEntity(new
                        {
                            Message = "Unable to perform the requested operation.",
                            Exception = e.Message,
                            ExtendedError = e.ExtendedErrorMessage,
                            e.ErrorCode
                        });
                    }
                }
            }

            return response;
        }
    }
}
