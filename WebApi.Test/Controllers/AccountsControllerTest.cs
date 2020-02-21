using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ClientApi.Controllers;
using ClientModel.DataAccess.Create.CreateAccount;
using ClientModel.DataAccess.Get.GetAccount;
using ClientModel.Dtos;
using ClientModel.Entities;
using ClientModel.Exceptions;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ClientApi.Test.Controllers
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
                },
                IdentityProviders = new List<IdentityProviderDto>
                {
                    new IdentityProviderDto
                    {
                        Name = "Health Dialog IdP"
                    }
                }
            };
        }

        public static TestServer CreateTestServer(Dictionary<string, string> configurationEntries = null)
        {
            configurationEntries = configurationEntries ?? new Dictionary<string, string>
            {
                ["ConnectionStrings:ClientsDbConnectionString"] = $"InMemory:{DateTime.Now}_{DateTime.UtcNow.Millisecond}",
                ["DisableAuthenticationAndAuthorization"] = "true",
                ["DisableHttpsRedirection"] = "true"
            };

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(configurationEntries).Build();
            var webhostBuilder = new WebHostBuilder().UseStartup<Startup>().UseConfiguration(configuration);

            return new TestServer(webhostBuilder);
        }

        [TestMethod]
        public async Task GivenAnAccountController_WhenIPassAValidAccountToTheCreateAccountMethod_ThenTheCallSucceedsWithHttp200()
        {
            // Setup: Create AccountsController and dependencies
            var server = CreateTestServer();
            var httpClient = server.CreateClient();

            // Exercise: Invoke AccountsController.CreateAccount with a valid AccountDto object
            var account = CreateValidAccountDefinition();
            var body = JsonConvert.SerializeObject(account);

            var response = await httpClient.PostAsync("/accounts", new StringContent(body, Encoding.UTF8, "application/json"));

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            
            var content = await response.Content.ReadAsStringAsync();
            content.Should().NotBeNullOrEmpty();

            var responseJson = string.IsNullOrEmpty(content) ? new JObject() : JToken.Parse(content);
            response.Should().NotBeNull();            
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
            createAccount.Setup(d => d.CreateAccountAsync(It.IsAny<AccountDto>())).ThrowsAsync(new ClientModelAggregateException("This is unfortunate =("));

            // System under test: Accounts Controller
            var accountsController = new AccountsController(createAccount.Object, getAccount.Object);

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
            var accountsController = new AccountsController(createAccount.Object, getAccount.Object);

            // Exercise: Invoke AccountsController.CreateAccount with a valid AccountDto object            
            var result = await accountsController.CreateAccount(account) as ObjectResult;

            Assert.IsNotNull(result, $"The {nameof(AccountsController.CreateAccount)} method returned an invalid result.");
            Assert.AreEqual(500, result.StatusCode);
        }
    }
}
