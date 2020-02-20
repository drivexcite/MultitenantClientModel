using System;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ClientApi.Authorization;
using ClientApi.ViewModels;
using ClientModel.DataAccess.Get.GetIdentityProviders;
using ClientModel.Exceptions;
using Microsoft.AspNetCore.Http;
using ClientModel.Dtos;
using ClientModel.DataAccess.Create.CreateIdentityProvider;
using System.Collections.Generic;

namespace ClientApi.Controllers
{
    [ApiController]
    public class IdentityProvidersController : ControllerBase
    {
        private readonly ILogger<SubscriptionsController> _logger;
        private readonly GetIdentityProvidersDelegate _getIdentityProviders;
        private readonly CreateIdentityProviderDelegate _createIdentityProvider;

        public IdentityProvidersController(ILogger<SubscriptionsController> logger, GetIdentityProvidersDelegate getIdentityProviders, CreateIdentityProviderDelegate createIdentityProvider)
        {
            _logger = logger;
            _getIdentityProviders = getIdentityProviders;
            _createIdentityProvider = createIdentityProvider;
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

        [HttpPost]
        [Route("accounts/{accountId}/identityProviders")]
        //[AuthorizeRbac("accounts:write")]
        public async Task<IActionResult> GetIdentityProviders(int accountId, IdentityProviderDto identityProvider)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";

            try
            {
                return Ok(await _createIdentityProvider.CreateIdentityProvider(accountId, identityProvider));
            }
            catch (AccountNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (MalformedAccountException e)
            {
                return BadRequest(e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError($"An unexpected error ocurred while processing POST: {baseUrl}?{Request.QueryString}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, new { result = $"An unexpected error occurred while creating a new identity provider for AccountId {accountId}" });
            }
        }

        [HttpGet]
        [Route("accounts/{accountId}/subscriptions/{subscriptionId}/identityProviders")]
        //[AuthorizeRbac("accounts:read")]
        public async Task<IActionResult> GetAccounts(int accountId, int subscriptionId, int skip = 0, int top = 10)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";

            try
            {
                var (items, total) = (new List<IdentityProviderDto>(), 0);
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
