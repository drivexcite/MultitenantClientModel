using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ClientApi.Filters;
using ClientApi.ViewModels;
using ClientModel.DataAccess.Create.CreateDataLink;
using ClientModel.DataAccess.Get.GetDataLink;
using ClientModel.Dtos;

namespace ClientApi.Controllers
{
    [ApiController]
    [TypeFilter(typeof(ClientModelExceptionFilter))]
    public class DataLinksController : ControllerBase
    {
        private readonly GetDataLinkDelegte _getDataLink;
        private readonly CreateDataLinkDelegate _createDataLink;

        public DataLinksController(GetDataLinkDelegte getDataLink, CreateDataLinkDelegate createDataLink)
        {
            _getDataLink = getDataLink;
            _createDataLink = createDataLink;
        }

        [HttpGet]
        [Route("accounts/{accountId}/dataLinks")]
        //[AuthorizeRbac("dataLinks:read")]
        public async Task<IActionResult> GetDataLinksForAccount(int accountId, int skip = 0, int top = 10)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";
            var (items, total) = await _getDataLink.GetDataLinksForAccountAsync(accountId, skip, top);

            return Ok(items.CreateServerSidePagedResult(baseUrl, total, skip, top));
        }

        [HttpGet]
        [Route("accounts/{accountId}/subscriptions/{subscriptionId}/dataLinks")]
        //[AuthorizeRbac("dataLinks:read")]
        public async Task<IActionResult> GetDataLinksForSubscription(int accountId, int subscriptionId, int skip = 0, int top = 10)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";
            var (items, total) = await _getDataLink.GetDataLinksForAccountAndSubscriptionAsync(accountId, subscriptionId, skip, top);

            return Ok(items.CreateServerSidePagedResult(baseUrl, total, skip, top));
        }

        [HttpPost]
        [Route("accounts/{accountId}/subscriptions/{subscriptionId}/dataLinks")]
        //[AuthorizeRbac("dataLinks:write")]
        public async Task<IActionResult> CreateDataLink(int accountId, int subscriptionId, [FromBody] DataLinkDto dataLink)
        {
            dataLink.FromSubscriptionId = subscriptionId;
            return Ok(await _createDataLink.CreateDataLinkAsync(accountId, dataLink));
        }
    }
}
