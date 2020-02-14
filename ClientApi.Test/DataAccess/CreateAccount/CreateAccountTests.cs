using AutoMapper;
using ClientApi.Dtos;
using ClientApi.Dtos.Mappings;
using ClientModel.DataAccess.CreateAccount;
using ClientModel.Entities;
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
        public async Task GivenAnAccountDefinition_WhenIHaveAValidAccount_ThenTheAccountShouldBePersisted()
        {
            // Setup: 1 Account with 2 Subscriptions
            using var db = ClientsDbTestProvider.CreateInMemoryClientDb();
            var dto = CreateValidAccountDefinition();

            // System under test: CreateAccountDelegate
            var createAccountDelegate = new CreateAccountDelegate(db, Mapper);

            // Exercise: invoke CreateAccount
            await createAccountDelegate.CreateAccount(dto);

            // Assert: the number of Accounts in the InMemory database is now 1
            // Assert: the number of Subscriptions in the InMemory database is now 2
            Assert.AreEqual(1, db.Accounts.Count(), "The number of Accounts in the ClientsDb is not 1");
            Assert.AreEqual(2, db.Subscriptions.Count(), "The number of Accounts in the ClientsDb is not 1");
        }

        private async Task AssertValidationErorOnAccountCreation(Action<AccountDto> alterDto, Action<Exception> assertOverThrownException, string explanation, bool useEntityModel = false)
        {
            // Setup: Create DTO instance and apply transformation
            var dto = CreateValidAccountDefinition();
            alterDto(dto);

            // System under test: CreateAccountDelegate
            var createAccountDelegate = new CreateAccountDelegate(useEntityModel ? ClientsDbTestProvider.CreateInMemoryClientDb() : null, Mapper);

            // Exercise: invoke CreateAccount
            try
            {
                await createAccountDelegate.CreateAccount(dto);
                Assert.Fail($"The invocation to {nameof(CreateAccountDelegate.CreateAccount)} should not succeed when {explanation}.");
            }
            catch (Exception e)
            {
                assertOverThrownException(e);
            }
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheDefinitionIsMissingTheAccountName_ThenAValidationErrorShouldBeRaised()
        {
            await AssertValidationErorOnAccountCreation(
                alterDto: dto => dto.AccountName = "", // Setup: Account Name is empty
                assertOverThrownException: e =>
                {
                    Assert.AreEqual(typeof(AggregateException), e.GetType());
                    var aggregateException = (AggregateException)e;

                    Assert.AreEqual(1, aggregateException.InnerExceptions.Count);
                    Assert.AreEqual($"The {nameof(AccountDto.AccountName)} field is required.", aggregateException.InnerExceptions[0].Message);
                },
                explanation: $"the {nameof(AccountDto.AccountName)} property is missing"
            );     
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheTheAccountNameIsLongerThan120_ThenAValidationErrorShouldBeRaised()
        {
            await AssertValidationErorOnAccountCreation(
                alterDto: dto => dto.AccountName = new string('e', 121), // Setup: 1 AccountName is just over 120 characters
                assertOverThrownException: e =>
                {
                    Assert.AreEqual(typeof(AggregateException), e.GetType());
                    var aggregateException = (AggregateException)e;

                    Assert.AreEqual(1, aggregateException.InnerExceptions.Count);
                    Assert.AreEqual($"The field {nameof(AccountDto.AccountName)} must be a string with a maximum length of 120.", aggregateException.InnerExceptions[0].Message);
                },
                explanation: $"the {nameof(AccountDto.AccountName)} property is larger than 120 characters"
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheTheAccountTypeIdIsMissing_ThenAValidationErrorShouldBeRaised()
        {
            await AssertValidationErorOnAccountCreation(
                alterDto: dto => dto.AccountTypeId = 0, // Setup: AccountTypeId is set to an invalid value
                assertOverThrownException: e =>
                {
                    Assert.AreEqual(typeof(AggregateException), e.GetType());
                    var aggregateException = (AggregateException)e;

                    Assert.AreEqual(1, aggregateException.InnerExceptions.Count);
                    Assert.AreEqual($"The account type with {nameof(AccountDto.AccountTypeId)} [0] is invalid", aggregateException.InnerExceptions[0].Message);
                },
                explanation: $"the {nameof(AccountDto.AccountType)} property has an invalid value",
                useEntityModel: true
            );
        }
    }
}
