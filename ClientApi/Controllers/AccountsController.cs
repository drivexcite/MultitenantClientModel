using ClientApi.ViewModels;
using ClientApi.Entities;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ClientApi.Controllers
{
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly IMapper _mapper;

        public AccountsController(IMapper mapper)
        {
            _mapper = mapper;
        }

        [HttpGet]
        [Route("accounts")]
        public async Task<IEnumerable<AccountViewModel>> GetAccounts(int top = 100, int skip = 0)
        {
            using var db = new ClientsDb();
            return await _mapper.ProjectTo<AccountViewModel>(db.Accounts.Skip(skip).Take(top)).ToListAsync();
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
            using var db = new ClientsDb();
            var (dependencies, errorMessage) = await CreateAccountDelegate.PrefetchAndValidateAsync(accountViewModel, db);

            if (errorMessage.Length > 0)
                return BadRequest(errorMessage.ToString());

            var account = await CreateAccountDelegate.PersistAccountAsync(accountViewModel, db, dependencies);

            return Ok(_mapper.Map<AccountViewModel>(account));
        }
    }
}
