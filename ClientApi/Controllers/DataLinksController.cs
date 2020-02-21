using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ClientApi.Controllers
{
    [ApiController]
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
