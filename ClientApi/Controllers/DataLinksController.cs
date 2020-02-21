using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ClientApi.Filters;

namespace ClientApi.Controllers
{
    [ApiController]
    [ServiceFilter(typeof(ClientModelExceptionFilter))]
    public class DataLinksController : ControllerBase
    {
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
