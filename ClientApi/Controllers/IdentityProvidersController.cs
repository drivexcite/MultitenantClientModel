using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ClientApi.Authorization;

namespace ClientApi.Controllers
{
    [ApiController]
    public class IdentityProvidersController : ControllerBase
    {
        private readonly ILogger<SubscriptionsController> _logger;

        public IdentityProvidersController(ILogger<SubscriptionsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("accounts/{accountId}/identityProviders")]
        //[AuthorizeRbac("accounts:read")]
        public async Task<IActionResult> GetAccounts()
        {
            return Ok();
        }
    }
}
