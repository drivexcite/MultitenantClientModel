using ClientApi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ClientApi.Filters;
using ClientModel.DataAccess.Create.CreateAccount;
using ClientModel.DataAccess.Get.GetAccount;
using ClientModel.Dtos;
using ClientApi.Authorization;

namespace ClientApi.Controllers
{
    [ApiController]
    [TypeFilter(typeof(ClientModelExceptionFilter))]
    public class AccountsController : ControllerBase
    {
        private readonly CreateAccountDelegate _createAccount;
        private readonly GetAccountDelegate _getAccount;

        public AccountsController(CreateAccountDelegate createAccount, GetAccountDelegate getAccount)
        {
            _createAccount = createAccount;
            _getAccount = getAccount;
        }

        [HttpGet]
        [Route("accounts")]
        [AuthorizeRbac("users:read")]
        public async Task<IActionResult> GetAccounts(int skip = 0, int top = 10)
        {
            var baseUrl = $"{Request?.Scheme}://{Request?.Host}{Request?.PathBase}{Request?.Path}";
            var (items, total) = await _getAccount.GetAccountsAsync(skip, top);

            return Ok(items.CreateServerSidePagedResult(baseUrl, total, skip, top));
        }

        [HttpGet]
        [Route("accounts/{accountId}")]
        //[AuthorizeRbac("accounts:read")]
        public async Task<IActionResult> GetAccount(int accountId)
        {
            return Ok(await _getAccount.GetAccountAsync(accountId));
        }

        [HttpPost]
        [Route("accounts")]
        //[AuthorizeRbac("accounts:write")]
        public async Task<IActionResult> CreateAccount(AccountDto accountViewModel)
        {
            return Ok(await _createAccount.CreateAccountAsync(accountViewModel));
        }
    }
}
