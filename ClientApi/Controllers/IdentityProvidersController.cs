using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ClientApi.Authorization;
using ClientApi.ViewModels;
using ClientModel.DataAccess.Get.GetIdentityProviders;
using ClientModel.Exceptions;
using Microsoft.AspNetCore.Http;

namespace ClientApi.Controllers
{
    [ApiController]
    public class IdentityProvidersController : ControllerBase
    {
        private readonly ILogger<SubscriptionsController> _logger;
        private readonly GetIdentityProvidersDelegate _getIdentityProviders;

        public IdentityProvidersController(ILogger<SubscriptionsController> logger, GetIdentityProvidersDelegate getIdentityProviders)
        {
            _logger = logger;
            _getIdentityProviders = getIdentityProviders;
        }

        [HttpGet]
        [Route("accounts/{accountId}/identityProviders")]
        //[AuthorizeRbac("accounts:read")]
        public async Task<IActionResult> GetIdentityProviders(int accountId, int skip = 0, int top = 10)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";

            try
            {
                var (items, total) = await _getIdentityProviders.GetIdentityProvidersAsync(accountId, skip, top);
                return Ok(items.CreateServerSidePagedResult(baseUrl, total, skip, top));
            }
            catch (AccountNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError($"An unexpected error ocurred while processing GET: {baseUrl}?{Request.QueryString}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, new { result = $"An unexpected error occurred while fetching the identity providers for AccountId {accountId}" });
            }
        }
    }
}
