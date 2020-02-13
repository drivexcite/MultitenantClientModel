using ClientApi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClientApi.Controllers.CreateAccount;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging;
using ClientApi.Exceptions;

namespace ClientApi.Controllers
{
    [ApiController]
    public class SubscriptionsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<SubscriptionsController> _logger;
        private readonly GetSubscriptionDelegate _getSubscription;

        public SubscriptionsController(IMapper mapper, ILogger<SubscriptionsController> logger, GetSubscriptionDelegate getSubscription)
        {
            _mapper = mapper;
            _logger = logger;
            _getSubscription = getSubscription;
        }

        [HttpGet]
        [Route("accounts/{accountId}/subscriptions")]
        public async Task<IActionResult> GetAccounts(int accountId, int skip = 0, int top = 10)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";

            try
            {
                var (subscriptions, total) = await _getSubscription.GetSubscriptions(accountId, skip, top);
                var items = await _mapper.ProjectTo<SubscriptionViewModel>(subscriptions).ToListAsync();                   
                var viewModel = new ServerSidePagedResult<SubscriptionViewModel>(items, baseUrl, total, skip, top).BuildViewModel();

                return Ok(viewModel);
            }
            catch(AccountNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch(Exception e)
            {
                _logger.LogError($"An unexpected error ocurred while processing GET: {baseUrl}?{Request.QueryString}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, new { result = $"An unexpected error ocurred while fetching the subscriptions for AccountId {accountId}" });
            }
        }
    }
}
