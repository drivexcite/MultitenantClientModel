using ClientApi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using AutoMapper;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ClientApi.Controllers.CreateAccount;
using ClientApi.Exceptions;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging;

namespace ClientApi.Controllers
{
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILogger<AccountsController> _logger;
        private readonly CreateAccountDelegate _createAccount;
        private readonly GetAccountDelegate _getAccount;

        public AccountsController(IMapper mapper, ILogger<AccountsController> logger, CreateAccountDelegate createAccount, GetAccountDelegate getAccount)
        {
            _mapper = mapper;
            _logger = logger;
            _createAccount = createAccount;
            _getAccount = getAccount;
        }

        [HttpGet]
        [Route("accounts")]
        public async Task<IActionResult> GetAccounts(int skip = 0, int top = 10)
        {
            var baseUrl = $"{Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}";

            try
            {
                var (accounts, total) = await _getAccount.GetAccounts(skip, top);
                var items = await _mapper.ProjectTo<AccountViewModel>(accounts).ToListAsync();                
                var viewModel = new ServerSidePagedResult<AccountViewModel>(items, baseUrl, total, skip, top).BuildViewModel();

                return Ok(viewModel);
            }
            catch(Exception e)
            {
                _logger.LogError($"An unexpected error ocurred while processing GET: {baseUrl}?{Request.QueryString}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, new { result = "An unexpected error ocurred while fetching the accounts" });
            }
        }

        /*
         * Example POST Body
        {
	        "AccountName": "Health Dialog",
	        "AccountTypeId": 1,
	        "ArchetypeId": 1,
	        "SalesforceAccountId": "SF00001",
	        "SalesforceAccountUrl": "https://healtwise.salesforce.com/accounts/SF00001",
	        "SalesforceAccountNumber": "",
	        "SalesforceAccountManager": "Katie Haller <khaller@healthwise.org>",
	        "ContractNumber": "HW.SF00001",
	        "Subscriptions": [{
		        "SubscriptionName": "Health Dialog - Main",
		        "Description": "Production subscription for Health Dialog",
		        "Tags": {
			        "Managed": "true",
			        "PHI": "true"

                },
		        "OrganizationalUnit": "Production",
		        "SubscriptionTypeId": 1
	        },{
		        "SubscriptionName": "Health Dialog - Staging",
		        "Description": "Production subscription for Health Dialog",
		        "Tags": {
			        "Managed": "true",
			        "PHI": "false"
		        },
		        "OrganizationalUnit": "Staging",
		        "SubscriptionTypeId": 2
	        }]
        }
        */
        [HttpPost]
        [Route("accounts")]
        public async Task<IActionResult> CreateAccount(AccountViewModel accountViewModel)
        {
            try
            {
                var dependencies = await _createAccount.PrefetchAndValidateAsync(accountViewModel);
                var account = await _createAccount.PersistAccountAsync(accountViewModel, dependencies);

                var viewModel = _mapper.Map<AccountViewModel>(account);

                _logger.LogInformation("Created Account {@ViewModel}", viewModel);

                return Ok(viewModel);
            }
            catch(AccountValidationException e)
            {
                var message = new
                {
                    result = e.Message,
                    details = (
                        from i in e.InnerExceptions ?? new List<System.Exception>()
                        select new { error = e.Message }
                    ).ToList()
                };

                return BadRequest(message);
            } 
            catch(PersistenceException e)
            {
                _logger.LogError($"An unexpected error ocurred while processing POST: {Request.Scheme}://{Request.Host}{Request.PathBase}{Request.Path}?{Request.QueryString}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, new { result = e.Message });
            }
        }
    }
}
