using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ClientApi.ViewModels;
using ClientModel.DataAccess.Get.GetIdentityProviders;
using ClientModel.Dtos;
using ClientModel.DataAccess.Create.CreateIdentityProvider;

namespace ClientApi.Controllers
{
    [ApiController]
    public class IdentityProvidersController : ControllerBase
    {
        private readonly GetIdentityProvidersDelegate _getIdentityProviders;
        private readonly CreateIdentityProviderDelegate _createIdentityProvider;

        public IdentityProvidersController(GetIdentityProvidersDelegate getIdentityProviders, CreateIdentityProviderDelegate createIdentityProvider)
        {
            _getIdentityProviders = getIdentityProviders;
            _createIdentityProvider = createIdentityProvider;
        }

        [HttpGet]
        [Route("accounts/{accountId}/identityProviders")]
        //[AuthorizeRbac("accounts:read")]
        public async Task<IActionResult> GetIdentityProviders(int accountId, int skip = 0, int top = 10)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";
            var (items, total) = await _getIdentityProviders.GetIdentityProvidersAsync(accountId, skip, top);

            return Ok(items.CreateServerSidePagedResult(baseUrl, total, skip, top));
        }

        [HttpPost]
        [Route("accounts/{accountId}/identityProviders")]
        //[AuthorizeRbac("accounts:write")]
        public async Task<IActionResult> GetIdentityProviders(int accountId, IdentityProviderDto identityProvider)
        {
            return Ok(await _createIdentityProvider.CreateIdentityProvider(accountId, identityProvider));
        }

        [HttpGet]
        [Route("accounts/{accountId}/subscriptions/{subscriptionId}/identityProviders")]
        //[AuthorizeRbac("accounts:read")]
        public async Task<IActionResult> GetIdentityProvidersForAccountAndSubscription(int accountId, int subscriptionId, int skip = 0, int top = 10)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";
            var (items, total) = await _getIdentityProviders.GetIdentityProvidersForSubscriptionAsync(accountId, subscriptionId, skip, top);

            return Ok(items.CreateServerSidePagedResult(baseUrl, total, skip, top));
        }

        [HttpPost]
        [Route("accounts/{accountId}/subscriptions/{subscriptionId}/identityProviders")]
        //[AuthorizeRbac("accounts:write")]
        public async Task<IActionResult> AssignIdentityProvidersToSubscription(int accountId, int subscriptionId, [FromBody]IdentityProviderAssignmentViewModel body, int skip = 0, int top = 10)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";

            if (body?.IdentityProviderIds == null || body.IdentityProviderIds.Length < 1)
                return BadRequest($"A list of valid Identity Providers must be supplied in the request body: {{ identityProviderIds: [...] }}");

            var identityProviderIds = body?.IdentityProviderIds?.ToList() ?? new List<int>();
            var (items, total) = await _createIdentityProvider.AssignIdentityProviderToSubscription(accountId, subscriptionId, identityProviderIds, skip, top);

            return Ok(items.CreateServerSidePagedResult(baseUrl, total, skip, top));
        }
    }
}
