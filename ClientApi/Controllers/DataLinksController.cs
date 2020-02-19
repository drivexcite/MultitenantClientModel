using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ClientApi.Authorization;

namespace ClientApi.Controllers
{
    [ApiController]
    public class DataLinksController : ControllerBase
    {
        private readonly ILogger<SubscriptionsController> _logger;
        

        public DataLinksController(ILogger<SubscriptionsController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("accounts/{accountId}/subscriptions/{subscriptionId}/dataLinks")]
        //[AuthorizeRbac("accounts:read")]
        public async Task<IActionResult> GetDataLinks()
        {
            return Ok();
        }

        [HttpPost]
        [Route("accounts/{accountId}/subscriptions/{subscriptionId}/dataLinks")]
        //[AuthorizeRbac("accounts:read")]
        public async Task<IActionResult> CreateDataLinks()
        {
            return Ok();
        }
    }
}
