using ClientApi.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ClientApi.Filters;
using ClientModel.DataAccess.Create.CreateAccount;
using ClientModel.DataAccess.Get.GetAccount;
using ClientModel.Dtos;
using ClientApi.Authorization;
using ClientModel.DataAccess.Update.UpdateAccount;
using ClientModel.Dtos.Update;

namespace ClientApi.Controllers
{
    [ApiController]
    [TypeFilter(typeof(ClientModelExceptionFilter))]
    public class AccountsController : ControllerBase
    {
        private readonly CreateAccountDelegate _createAccount;
        private readonly GetAccountDelegate _getAccount;
        private readonly UpdateAccountDelegate _updateAccount;

        public AccountsController(CreateAccountDelegate createAccount, GetAccountDelegate getAccount, UpdateAccountDelegate updateAccount)
        {
            _createAccount = createAccount;
            _getAccount = getAccount;
            _updateAccount = updateAccount;
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
        public async Task<IActionResult> CreateAccount(AccountDto accountDto)
        {
            accountDto = await _createAccount.CreateAccountAsync(accountDto);
            var accountUrl = $"{Request?.Scheme}://{Request?.Host}{Request?.PathBase}{Request?.Path}/{accountDto.AccountId}";

            return Created(accountUrl, accountDto);
        }

        [HttpPatch]
        [Route("accounts/{accountId}")]
        //[AuthorizeRbac("accounts:write")]
        public async Task<IActionResult> UpdateAccount(int accountId, UpdateAccountDto accountDto)
        {
            return Ok(await _updateAccount.UpdateAccountAsync(accountId, accountDto));
        }
    }
}
