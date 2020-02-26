using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ClientModel.Dtos;
using ClientModel.Entities;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            var databaseName = $"InMemory:{DateTime.Now}_{DateTime.UtcNow.Millisecond}";

            if (configurationEntries != null && configurationEntries.TryGetValue("ConnectionStrings:ClientsDbConnectionString", out var connectionstring) == true)
            {
                databaseName = connectionstring;
            }

            configurationEntries ??= new Dictionary<string, string>
            {
                ["ConnectionStrings:ClientsDbConnectionString"] = databaseName,
                ["DisableAuthenticationAndAuthorization"] = "true",
                ["DisableHttpsRedirection"] = "true"
            };

            if (databaseName.StartsWith("InMemory:", StringComparison.OrdinalIgnoreCase))
            {
                var options = new DbContextOptionsBuilder<ClientsDb>()
                    .UseInMemoryDatabase(databaseName: databaseName.Replace("InMemory:", ""))
                    .Options;

                using var db = new ClientsDb(options);

                db.AccountTypes.Add(new AccountType { AccountTypeId = 1, Name = "Client", CreatedDate = DateTime.UtcNow, CreatedBy = "TomStChief" });
                db.AccountTypes.Add(new AccountType { AccountTypeId = 2, Name = "Partner", CreatedDate = DateTime.UtcNow, CreatedBy = "TomStChief" });
                db.AccountTypes.Add(new AccountType { AccountTypeId = 3, Name = "Referral", CreatedDate = DateTime.UtcNow, CreatedBy = "TomStChief" });

                db.Archetypes.Add(new Archetype { ArchetypeId = 1, Name = "Basic", CreatedDate = DateTime.UtcNow, CreatedBy = "TomStChief" });
                db.Archetypes.Add(new Archetype { ArchetypeId = 2, Name = "Segregated", CreatedDate = DateTime.UtcNow, CreatedBy = "TomStChief" });
                db.Archetypes.Add(new Archetype { ArchetypeId = 3, Name = "Var", CreatedDate = DateTime.UtcNow, CreatedBy = "TomStChief" });
                db.Archetypes.Add(new Archetype { ArchetypeId = 4, Name = "Hybrid", CreatedDate = DateTime.UtcNow, CreatedBy = "TomStChief" });
                db.Archetypes.Add(new Archetype { ArchetypeId = 5, Name = "Enterprise", CreatedDate = DateTime.UtcNow, CreatedBy = "TomStChief" });

                db.SubscriptionTypes.Add(new SubscriptionType { SubscriptionTypeId = 1, Name = "Production", CreatedDate = DateTime.UtcNow, CreatedBy = "TomStChief" });
                db.SubscriptionTypes.Add(new SubscriptionType { SubscriptionTypeId = 2, Name = "Test", CreatedDate = DateTime.UtcNow, CreatedBy = "TomStChief" });
                db.SubscriptionTypes.Add(new SubscriptionType { SubscriptionTypeId = 3, Name = "Demo", CreatedDate = DateTime.UtcNow, CreatedBy = "TomStChief" });

                db.DataLinkTypes.Add(new DataLinkType { DataLinkTypeId = 1, Name = "Customization", CreatedDate = DateTime.UtcNow, CreatedBy = "TomStChief" });
                db.DataLinkTypes.Add(new DataLinkType { DataLinkTypeId = 2, Name = "Activity", CreatedDate = DateTime.UtcNow, CreatedBy = "TomStChief" });

                db.SaveChanges();
            }

            var configuration = new ConfigurationBuilder().AddInMemoryCollection(configurationEntries).Build();
            var webhostBuilder = new WebHostBuilder().UseStartup<Startup>().UseConfiguration(configuration);

            return new TestServer(webhostBuilder);
        }

        private void AssertThatAccountJsonResemblesDto(JToken accountJson, AccountDto account)
        {
            accountJson.Should().NotBeNull();

            account.GetType().GetProperties().Length.Should().Be(accountJson.Count());

            accountJson["accountId"].Should().NotBeNull().And.BeOfType<JValue>();
            accountJson["accountId"].Value<int>().Should().NotBe(0);

            accountJson["accountTypeId"].Should().NotBeNull().And.BeOfType<JValue>();
            accountJson["accountTypeId"].Value<int>().Should().Be(account.AccountTypeId);

            accountJson["archetypeId"].Should().NotBeNull().And.BeOfType<JValue>();
            accountJson["archetypeId"].Value<int>().Should().Be(account.ArchetypeId);

            accountJson["salesforceAccountId"].Should().NotBeNull().And.BeOfType<JValue>();
            accountJson["salesforceAccountId"].Value<string>().Should().NotBeNullOrEmpty().And.Be(account.SalesforceAccountId);

            accountJson["salesforceAccountManager"].Should().NotBeNull().And.BeOfType<JValue>();
            accountJson["salesforceAccountManager"].Value<string>().Should().NotBeNullOrEmpty().And.Be(account.SalesforceAccountManager);

            accountJson["salesforceAccountNumber"].Should().NotBeNull().And.BeOfType<JValue>();
            accountJson["salesforceAccountNumber"].Value<string>().Should().BeNullOrEmpty();

            accountJson["salesforceAccountUrl"].Should().NotBeNull().And.BeOfType<JValue>();
            accountJson["salesforceAccountUrl"].Value<string>().Should().NotBeNullOrEmpty().And.Be(account.SalesforceAccountUrl);

            accountJson["contractNumber"].Should().NotBeNull().And.BeOfType<JValue>();
            accountJson["contractNumber"].Value<string>().Should().NotBeNullOrEmpty().And.Be(account.ContractNumber);

            accountJson["subscriptions"].Should().BeOfType<JArray>().And.NotBeNullOrEmpty().And.HaveCount(account.Subscriptions.Count);

            for (var i = 0; i < account.Subscriptions.Count; i++)
            {
                var subscription = account.Subscriptions[i];
                var subscriptionJson = ((JArray)accountJson["subscriptions"])[i];

                subscription.GetType().GetProperties().Length.Should().Be(subscriptionJson.Count());

                subscriptionJson["subscriptionName"].Should().NotBeNull().And.BeOfType<JValue>();
                subscriptionJson["subscriptionName"].Value<string>().Should().NotBeNullOrEmpty().And.Be(subscription.SubscriptionName);

                subscriptionJson["description"].Should().NotBeNull().And.BeOfType<JValue>();
                subscriptionJson["description"].Value<string>().Should().NotBeNullOrEmpty().And.Be(subscription.Description);

                subscriptionJson["organizationalUnit"].Should().NotBeNull().And.BeOfType<JValue>();
                subscriptionJson["organizationalUnit"].Value<string>().Should().NotBeNullOrEmpty().And.Be(subscription.OrganizationalUnit);

                subscriptionJson["subscriptionTypeId"].Should().NotBeNull().And.BeOfType<JValue>();
                subscriptionJson["subscriptionTypeId"].Value<int>().Should().Be(subscription.SubscriptionTypeId);

                subscriptionJson["tags"].Should().BeOfType<JObject>().And.NotBeNullOrEmpty().And.HaveCount(subscription.Tags.Count);
            }

            accountJson["identityProviders"].Should().BeOfType<JArray>().And.NotBeNullOrEmpty().And.HaveCount(account.IdentityProviders.Count);
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

            var accountJson = string.IsNullOrEmpty(content) ? new JObject() : JToken.Parse(content);
            AssertThatAccountJsonResemblesDto(accountJson, account);
        }
    }
}
