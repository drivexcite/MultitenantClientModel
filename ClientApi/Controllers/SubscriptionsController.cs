using ClientApi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging;
using ClientApi.Authorization;
using ClientModel.DataAccess.Get.GetSubscriptions;
using ClientModel.Exceptions;

namespace ClientApi.Controllers
{
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly ILogger<SubscriptionsController> _logger;
        private readonly GetSubscriptionDelegate _getSubscription;

        public SubscriptionsController(ILogger<SubscriptionsController> logger, GetSubscriptionDelegate getSubscription)
        {
            _logger = logger;
            _getSubscription = getSubscription;
        }

        [HttpGet]
        [Route("accounts/{accountId}/subscriptions")]
        //[AuthorizeRbac("accounts:read")]
        public async Task<IActionResult> GetAccounts(int accountId, int skip = 0, int top = 10)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";

            try
            {
                var (items, total) = await _getSubscription.GetSubscriptionsAsync(accountId, skip, top);
                return Ok(items.CreateServerSidePagedResult(baseUrl, total, skip, top));
            }
            catch (AccountNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError($"An unexpected error ocurred while processing GET: {baseUrl}?{Request.QueryString}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, new { result = $"An unexpected error occurred while fetching the subscriptions for AccountId {accountId}" });
            }
        }
    }
}
