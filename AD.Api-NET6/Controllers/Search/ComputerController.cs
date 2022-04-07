using AD.Api.Ldap.Filters;
using AD.Api.Services;
using AD.Api.Settings;
using Microsoft.AspNetCore.Mvc;

namespace AD.Api.Controllers.Search
{
    [ApiController]
    [Produces("application/json")]
    [Route("search/computer")]
    public class ComputerQueryController : ADQueryController
    {
        private static readonly Equal ComputerObjectClass = new Equal("objectClass", "computer");

        private IGenericSettings GenericSettings { get; }
        private IComputerSettings ComputerSettings { get; }

        public ComputerQueryController(IConnectionService connectionService, IGenericSettings genericSettings,
            IComputerSettings computerSettings, ISerializationService serializer)
            : base(connectionService, serializer)
        {
            this.GenericSettings = genericSettings;
            this.ComputerSettings = computerSettings;
        }

        
    }
}
