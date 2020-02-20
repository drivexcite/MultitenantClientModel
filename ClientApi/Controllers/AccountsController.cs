using ClientApi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.Logging;
using ClientModel.DataAccess.Create.CreateAccount;
using ClientModel.DataAccess.Get.GetAccount;
using ClientModel.Dtos;
using ClientModel.Exceptions;

namespace ClientApi.Controllers
{
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly ILogger<AccountsController> _logger;
        private readonly CreateAccountDelegate _createAccount;
        private readonly GetAccountDelegate _getAccount;

        public AccountsController(ILogger<AccountsController> logger, CreateAccountDelegate createAccount, GetAccountDelegate getAccount)
        {
            _logger = logger;
            _createAccount = createAccount;
            _getAccount = getAccount;
        }

        [HttpGet]
        [Route("accounts")]
        //[AuthorizeRbac("accounts:read")]
        public async Task<IActionResult> GetAccounts(int skip = 0, int top = 10)
        {
            var baseUrl = $"{Request?.Scheme}://{Request?.Host}{Request?.PathBase}{Request?.Path}";

            try
            {
                var (items, total) = await _getAccount.GetAccountsAsync(skip, top);
                return Ok(items.CreateServerSidePagedResult(baseUrl, total, skip, top));
            }
            catch (Exception e)
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
        //[AuthorizeRbac("accounts:create")]
        public async Task<IActionResult> CreateAccount(AccountDto accountViewModel)
        {
            try
            {
                var account = await _createAccount.CreateAccountAsync(accountViewModel);
                _logger.LogInformation("Created Account {@ViewModel}", account);

                return Ok(account);
            }
            catch (AggregateException e)
            {
                var message = new
                {
                    result = e.Message,
                    details = (
                        from i in e.InnerExceptions
                        select new { error = e.Message }
                    ).ToList()
                };

                return BadRequest(message);
            }
            catch (PersistenceException e)
            {
                _logger.LogError($"An unexpected error ocurred while processing POST: {Request?.Scheme}://{Request?.Host}{Request?.PathBase}{Request?.Path}?{Request?.QueryString}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, new { result = e.Message });
            }
        }
    }
}
