using ClientApi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ClientApi.Filters;
using ClientModel.DataAccess.Get.GetSubscriptions;

namespace ClientApi.Controllers
{
    [ApiController]
    [ServiceFilter(typeof(ClientModelExceptionFilter))]
    public class SubscriptionsController : ControllerBase
    {
        private readonly GetSubscriptionDelegate _getSubscription;

        public SubscriptionsController(GetSubscriptionDelegate getSubscription)
        {
            _getSubscription = getSubscription;
        }

        [HttpGet]
        [Route("accounts/{accountId}/subscriptions")]
        //[AuthorizeRbac("accounts:read")]
        public async Task<IActionResult> GetAccounts(int accountId, int skip = 0, int top = 10)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";
            var (items, total) = await _getSubscription.GetSubscriptionsAsync(accountId, skip, top);

            return Ok(items.CreateServerSidePagedResult(baseUrl, total, skip, top));
        }
    }
}
