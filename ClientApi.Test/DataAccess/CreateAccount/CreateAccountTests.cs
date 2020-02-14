using AutoMapper;
using ClientApi.Dtos;
using ClientApi.Dtos.Mappings;
using ClientModel.DataAccess.CreateAccount;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            var memberInfo = typeof(AccountDto).GetProperty(nameof(AccountDto.AccountName));
            var attribute = (StringLengthAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(StringLengthAttribute));
            var length = attribute.MaximumLength;

            await AssertValidationErorOnAccountCreation(
                alterDto: dto => dto.AccountName = new string('e', length + 1), // Setup: 1 AccountName is just over 120 characters
                assertOverThrownException: e =>
                {
                    Assert.AreEqual(typeof(AggregateException), e.GetType());
                    var aggregateException = (AggregateException)e;

                    Assert.AreEqual(1, aggregateException.InnerExceptions.Count);
                    Assert.AreEqual($"The field {nameof(AccountDto.AccountName)} must be a string with a maximum length of {length}.", aggregateException.InnerExceptions[0].Message);
                },
                explanation: $"the {nameof(AccountDto.AccountName)} property is larger than {length} characters"
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
                explanation: $"the {nameof(AccountDto.AccountTypeId)} property has an invalid value",
                useEntityModel: true
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheTheArchetypeIdIsMissing_ThenAValidationErrorShouldBeRaised()
        {
            await AssertValidationErorOnAccountCreation(
                alterDto: dto => dto.ArchetypeId = 0, // Setup: AccountTypeId is set to an invalid value
                assertOverThrownException: e =>
                {
                    Assert.AreEqual(typeof(AggregateException), e.GetType());
                    var aggregateException = (AggregateException)e;

                    Assert.AreEqual(1, aggregateException.InnerExceptions.Count);
                    Assert.AreEqual($"The archetype with {nameof(AccountDto.ArchetypeId)} [0] is invalid", aggregateException.InnerExceptions[0].Message);
                },
                explanation: $"the {nameof(AccountDto.ArchetypeId)} property has an invalid value",
                useEntityModel: true
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheTheSalesforceAccountIdIsLongerThan40_ThenAValidationErrorShouldBeRaised()
        {
            var memberInfo = typeof(AccountDto).GetProperty(nameof(AccountDto.SalesforceAccountId));
            var attribute = (StringLengthAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(StringLengthAttribute));
            var length = attribute.MaximumLength;

            await AssertValidationErorOnAccountCreation(
                alterDto: dto => dto.SalesforceAccountId = new string('e', length + 1), // Setup: 1 SalesforceAccountId is just over 40 characters
                assertOverThrownException: e =>
                {
                    Assert.AreEqual(typeof(AggregateException), e.GetType());
                    var aggregateException = (AggregateException)e;

                    Assert.AreEqual(1, aggregateException.InnerExceptions.Count);
                    Assert.AreEqual($"The field {nameof(AccountDto.SalesforceAccountId)} must be a string with a maximum length of {length}.", aggregateException.InnerExceptions[0].Message);
                },
                explanation: $"the {nameof(AccountDto.SalesforceAccountId)} property is larger than {length} characters"
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheTheSalesforceAccountNumberIsLongerThan40_ThenAValidationErrorShouldBeRaised()
        {
            var memberInfo = typeof(AccountDto).GetProperty(nameof(AccountDto.SalesforceAccountNumber));
            var attribute = (StringLengthAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(StringLengthAttribute));
            var length = attribute.MaximumLength;

            await AssertValidationErorOnAccountCreation(
                alterDto: dto => dto.SalesforceAccountNumber = new string('e', length + 1), // Setup: 1 SalesforceAccountNumber is just over 40 characters
                assertOverThrownException: e =>
                {
                    Assert.AreEqual(typeof(AggregateException), e.GetType());
                    var aggregateException = (AggregateException)e;

                    Assert.AreEqual(1, aggregateException.InnerExceptions.Count);
                    Assert.AreEqual($"The field {nameof(AccountDto.SalesforceAccountNumber)} must be a string with a maximum length of {length}.", aggregateException.InnerExceptions[0].Message);
                },
                explanation: $"the {nameof(AccountDto.SalesforceAccountNumber)} property is larger than {length} characters"
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheTheSalesforceAccountManagerIsLongerThan120_ThenAValidationErrorShouldBeRaised()
        {
            var memberInfo = typeof(AccountDto).GetProperty(nameof(AccountDto.SalesforceAccountManager));
            var attribute = (StringLengthAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(StringLengthAttribute));
            var length = attribute.MaximumLength;

            await AssertValidationErorOnAccountCreation(
                alterDto: dto => dto.SalesforceAccountManager = new string('e', length + 1), // Setup: 1 SalesforceAccountManager is just over 120 characters
                assertOverThrownException: e =>
                {
                    Assert.AreEqual(typeof(AggregateException), e.GetType());
                    var aggregateException = (AggregateException)e;

                    Assert.AreEqual(1, aggregateException.InnerExceptions.Count);
                    Assert.AreEqual($"The field {nameof(AccountDto.SalesforceAccountManager)} must be a string with a maximum length of {length}.", aggregateException.InnerExceptions[0].Message);
                },
                explanation: $"the {nameof(AccountDto.SalesforceAccountManager)} property is larger than {length} characters"
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheTheContractNumberIsLongerThan40_ThenAValidationErrorShouldBeRaised()
        {
            var memberInfo = typeof(AccountDto).GetProperty(nameof(AccountDto.ContractNumber));
            var attribute = (StringLengthAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(StringLengthAttribute));
            var length = attribute.MaximumLength;

            await AssertValidationErorOnAccountCreation(
                alterDto: dto => dto.ContractNumber = new string('e', length + 1), // Setup: 1 ContractNumber is just over 40 characters
                assertOverThrownException: e =>
                {
                    Assert.AreEqual(typeof(AggregateException), e.GetType());
                    var aggregateException = (AggregateException)e;

                    Assert.AreEqual(1, aggregateException.InnerExceptions.Count);
                    Assert.AreEqual($"The field {nameof(AccountDto.ContractNumber)} must be a string with a maximum length of {length}.", aggregateException.InnerExceptions[0].Message);
                },
                explanation: $"the {nameof(AccountDto.ContractNumber)} property is larger than {length} characters"
            );
        }
    }
}
