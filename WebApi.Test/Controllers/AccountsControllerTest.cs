using AutoMapper;
using ClientApi.Controllers;
using ClientApi.Dtos;
using ClientModel.DataAccess.CreateAccount;
using ClientModel.DataAccess.GetAccount;
using ClientModel.Entities;
using ClientModel.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace WebApi.Test
{
    [TestClass]
    public class AccountsControllerTest
    {
        public static AccountDto CreateValidAccountDefinition()
        {
            return new AccountDto
            {
                AccountName = "Health Dialog",
                AccountTypeId = 1,
                ArchetypeId = 1,
                SalesforceAccountId = "SF00001",
                SalesforceAccountUrl = "https://healtwise.salesforce.com/accounts/SF00001",
                SalesforceAccountNumber = "",
                SalesforceAccountManager = "Katie Haller <khaller@healthwise.org>",
                ContractNumber = "HW.SF00001",
                Subscriptions = new List<SubscriptionDto>
                {
                    new SubscriptionDto {
                        SubscriptionName = "Health Dialog - Main",
                        Description = "Production subscription for Health Dialog",
                        Tags = new Dictionary<string, string> {
                            ["Managed"] = "true",
                            ["PHI"] = "true"
                        },
                        OrganizationalUnit = "Production",
                        SubscriptionTypeId = 1
                    },
                    new SubscriptionDto {
                        SubscriptionName = "Health Dialog - Staging",
                        Description = "Production subscription for Health Dialog",
                        Tags = new Dictionary<string, string> {
                            ["Managed"] = "true",
                            ["PHI"] = "false"
                        },
                        OrganizationalUnit = "Staging",
                        SubscriptionTypeId = 2
                    }
                }
            };
        }

        [TestMethod]
        public async Task GivenAnAccountController_WhenIPassAValidAccountToTheCreateAccountMethod_ThenTheCallSucceedsWithHttp200()
        {
            // Setup: Create AccountsController and dependencies
            var logger = new Mock<ILogger<AccountsController>>();
            var mapper = new Mock<IMapper>();
            var db = new Mock<ClientsDb>();            

            var createAccount = new Mock<CreateAccountDelegate>(db.Object, mapper.Object);
            var getAccount = new Mock<GetAccountDelegate>(db.Object, mapper.Object);

            var account = CreateValidAccountDefinition();

            // Setup: Simulate the Create Account data access method executed successfully.
            createAccount.Setup(d => d.CreateAccountAsync(It.IsAny<AccountDto>())).ReturnsAsync(account);

            // System under test: Accounts Controller
            var accountsController = new AccountsController(logger.Object, createAccount.Object, getAccount.Object);

            // Exercise: Invoke AccountsController.CreateAccount with a valid AccountDto object            
            var result = await accountsController.CreateAccount(account) as ObjectResult;

            Assert.IsNotNull(result, $"The {nameof(AccountsController.CreateAccount)} method returned an invalid result. Expected: {nameof(OkObjectResult)}");
            Assert.AreEqual(200, result.StatusCode);
        }

        [TestMethod]
        public async Task GivenAnAccountController_WhenIPassAnInvalidAccountToTheCreateAccountMethod_ThenTheCallSucceedsWithHttp400()
        {
            // Setup: Create AccountsController and dependencies
            var logger = new Mock<ILogger<AccountsController>>();
            var mapper = new Mock<IMapper>();
            var db = new Mock<ClientsDb>();

            var createAccount = new Mock<CreateAccountDelegate>(db.Object, mapper.Object);
            var getAccount = new Mock<GetAccountDelegate>(db.Object, mapper.Object);

            var account = CreateValidAccountDefinition();

            // Setup: Simulate the Create Account data access method throws an exception (validation exception).
            createAccount.Setup(d => d.CreateAccountAsync(It.IsAny<AccountDto>())).ThrowsAsync(new AggregateException("This is unfortunate =("));

            // System under test: Accounts Controller
            var accountsController = new AccountsController(logger.Object, createAccount.Object, getAccount.Object);

            // Exercise: Invoke AccountsController.CreateAccount with a valid AccountDto object            
            var result = await accountsController.CreateAccount(account) as ObjectResult;

            Assert.IsNotNull(result, $"The {nameof(AccountsController.CreateAccount)} method returned an invalid result.");
            Assert.AreEqual(400, result.StatusCode);
        }

        [TestMethod]
        public async Task GivenAnAccountController_WhenThereIsADataAccessErrorOnTheCreateAccountMethod_ThenTheCallSucceedsWithHttp500()
        {
            // Setup: Create AccountsController and dependencies
            var logger = new Mock<ILogger<AccountsController>>();
            var mapper = new Mock<IMapper>();
            var db = new Mock<ClientsDb>();

            var createAccount = new Mock<CreateAccountDelegate>(db.Object, mapper.Object);
            var getAccount = new Mock<GetAccountDelegate>(db.Object, mapper.Object);

            var account = CreateValidAccountDefinition();

            // Setup: Simulate the Create Account data access method throws an exception (data access exception).
            createAccount.Setup(d => d.CreateAccountAsync(It.IsAny<AccountDto>())).ThrowsAsync(new PersistenceException("This is very unfortunate =("));

            // System under test: Accounts Controller
            var accountsController = new AccountsController(logger.Object, createAccount.Object, getAccount.Object);            

            // Exercise: Invoke AccountsController.CreateAccount with a valid AccountDto object            
            var result = await accountsController.CreateAccount(account) as ObjectResult;

            Assert.IsNotNull(result, $"The {nameof(AccountsController.CreateAccount)} method returned an invalid result.");
            Assert.AreEqual(500, result.StatusCode);
        }
    }
}
