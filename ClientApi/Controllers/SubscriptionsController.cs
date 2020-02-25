using ClientApi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ClientApi.Filters;
using ClientModel.DataAccess.Create.CreateSubscription;
using ClientModel.DataAccess.Get.GetSubscriptions;
using ClientModel.Dtos;

namespace ClientApi.Controllers
{
    [ApiController]
    [TypeFilter(typeof(ClientModelExceptionFilter))]
    public class SubscriptionsController : ControllerBase
    {
        private readonly GetSubscriptionDelegate _getSubscription;
        private readonly CreateSubscriptionDelegate _createSubscriptionDelegate;

        public SubscriptionsController(GetSubscriptionDelegate getSubscription, CreateSubscriptionDelegate createSubscriptionDelegate)
        {
            _getSubscription = getSubscription;
            _createSubscriptionDelegate = createSubscriptionDelegate;
        }

        [HttpGet]
        [Route("accounts/{accountId}/subscriptions")]
        //[AuthorizeRbac("accounts:read")]
        public async Task<IActionResult> GetSubscriptions(int accountId, int skip = 0, int top = 10)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";
            var (items, total) = await _getSubscription.GetSubscriptionsAsync(accountId, skip, top);

            return Ok(items.CreateServerSidePagedResult(baseUrl, total, skip, top));
        }

        [HttpPost]
        [Route("accounts/{accountId}/subscriptions")]
        //[AuthorizeRbac("accounts:write")]
        public async Task<IActionResult> CreateSubscription(int accountId, [FromBody]SubscriptionDto subscriptionDto)
        {
            return Ok(await _createSubscriptionDelegate.CreateSubscriptionAsync(accountId, subscriptionDto));
        }

        [HttpGet]
        [Route("accounts/{accountId}/subscriptions/{subscriptionId}")]
        //[AuthorizeRbac("accounts:read")]
        public async Task<IActionResult> GetSubscription(int accountId, int subscriptionId)
        {
            return Ok(await _getSubscription.GetSubscriptionAsync(accountId, subscriptionId));
        }
    }
}
