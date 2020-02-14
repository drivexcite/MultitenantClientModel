using AutoMapper;
using ClientApi.Dtos;
using ClientApi.Dtos.Mappings;
using ClientModel.DataAccess.CreateAccount;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientModel.Test
{
    [TestClass]
    public class CreateAccountTests
    {
        static IMapper Mapper { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            var profiles = from t in typeof(AccountProfile).Assembly.GetTypes()
                           where typeof(Profile).IsAssignableFrom(t)
                           select (Profile)Activator.CreateInstance(t);

            var configuration = new MapperConfiguration(config =>
            {
                foreach (var profile in profiles)
                {
                    config.AddProfile(profile);
                }
            });

            Mapper = configuration.CreateMapper();
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenIHaveAValidAccount_ThenTheAccountShouldBePersisted()
        {
            // Setup: 1 Account with 2 Subscriptions
            var db = ClientsDbTestProvider.CreateInMemoryClientDb();
            var dto = new AccountDto
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

            // System under test: CreateAccountDelegate
            var createAccountDelegate = new CreateAccountDelegate(db, Mapper);

            // Exercise: invoke CreateAccount
            await createAccountDelegate.CreateAccount(dto);

            // Assert: the number of Accounts in the InMemory database is now 1
            // Assert: the number of Subscriptions in the InMemory database is now 2
            Assert.AreEqual(1, db.Accounts.Count(), "The number of Accounts in the ClientsDb is not 1");
            Assert.AreEqual(2, db.Subscriptions.Count(), "The number of Accounts in the ClientsDb is not 1");
        }
    }
}
