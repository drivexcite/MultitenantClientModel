using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClientModel.DataAccess.Create.CreateAccount;
using ClientModel.Dtos;
using ClientModel.Exceptions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientModel.Test.DataAccess.CreateAccount
{
    [TestClass]
    public class CreateAccountTests
    {
        static IMapper Mapper { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Mapper = ClientModelMapperProvider.CreateAutoMapper();
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenIHaveAValidAccount_ThenTheAccountShouldBePersisted()
        {
            // Setup: 1 Account with 2 Subscriptions
            await using var db = ClientsDbTestProvider.CreateInMemoryClientDb();
            var account = DtoProvider.CreateValidAccountDefinition();

            // System under test: CreateAccountDelegate
            var createAccountDelegate = new CreateAccountDelegate(db, Mapper);

            // Exercise: invoke CreateAccount
            account = await createAccountDelegate.CreateAccountAsync(account);

            // Assert: the number of Accounts in the InMemory database is now 1
            // Assert: the number of Subscriptions in the InMemory database is now 2
            db.Accounts.Count().Should().Be(1, $"The number of {nameof(db.Accounts)} in the ClientsDb is not 1");
            db.Subscriptions.Count().Should().Be(2, $"The number of {nameof(db.Subscriptions)} in the ClientsDb is not 2");
            db.IdentityProviders.Count().Should().Be(1, $"The number of {nameof(db.IdentityProviders)} in the ClientsDb is not 1");

            account.AccountId.Should().NotBe(default, $"The underlying provider should generate a value for {nameof(AccountDto.AccountId)}");
        }

        private async Task AssertValidationErrorOnAccountCreation(Action<AccountDto> alterDto, Action<Exception> assertOverThrownException, string explanation, bool useEntityModel = false)
        {
            // Setup: Create DTO instance and apply transformation
            var dto = DtoProvider.CreateValidAccountDefinition();
            alterDto(dto);

            // System under test: CreateAccountDelegate
            var createAccountDelegate = new CreateAccountDelegate(useEntityModel ? ClientsDbTestProvider.CreateInMemoryClientDb() : null, Mapper);

            // Exercise: invoke CreateAccount
            try
            {
                await createAccountDelegate.CreateAccountAsync(dto);
                Assert.Fail($"The invocation to {nameof(CreateAccountDelegate.CreateAccountAsync)} should not succeed when {explanation}.");
            }
            catch (Exception e)
            {
                assertOverThrownException(e);
            }
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheDefinitionIsMissingTheAccountName_ThenAValidationErrorShouldBeRaised()
        {
            await AssertValidationErrorOnAccountCreation(
                alterDto: dto => dto.AccountName = "", // Setup: Account Name is empty
                assertOverThrownException: e =>
                {
                    e.GetType().Should().Be<ClientModelAggregateException>();
                    var aggregateException = (ClientModelAggregateException)e;

                    aggregateException.InnerExceptions.Count.Should().Be(1);
                    aggregateException.InnerExceptions[0].Message.Should().Be($"The {nameof(AccountDto.AccountName)} field is required.");
                },
                explanation: $"the {nameof(AccountDto.AccountName)} property is missing"
            );     
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheTheAccountNameIsLongerThan120_ThenAValidationErrorShouldBeRaised()
        {
            var memberInfo = typeof(AccountDto).GetProperty(nameof(AccountDto.AccountName));
            Assert.IsNotNull(memberInfo);

            var attribute = (StringLengthAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(StringLengthAttribute));
            var length = attribute.MaximumLength;

            await AssertValidationErrorOnAccountCreation(
                alterDto: dto => dto.AccountName = new string('e', length + 1), // Setup: 1 AccountName is just over 120 characters
                assertOverThrownException: e =>
                {
                    e.GetType().Should().Be<ClientModelAggregateException>();
                    var aggregateException = (ClientModelAggregateException)e;

                    aggregateException.InnerExceptions.Count.Should().Be(1);
                    aggregateException.InnerExceptions[0].Message.Should().Be($"The field {nameof(AccountDto.AccountName)} must be a string with a maximum length of {length}.");
                },
                explanation: $"the {nameof(AccountDto.AccountName)} property is larger than {length} characters"
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheTheAccountTypeIdIsMissing_ThenAValidationErrorShouldBeRaised()
        {
            await AssertValidationErrorOnAccountCreation(
                alterDto: dto => dto.AccountTypeId = 0, // Setup: AccountTypeId is set to an invalid value
                assertOverThrownException: e =>
                {
                    e.GetType().Should().Be<ClientModelAggregateException>();
                    var aggregateException = (ClientModelAggregateException)e;

                    aggregateException.InnerExceptions.Count.Should().Be(1);
                    aggregateException.InnerExceptions[0].Message.Should().Be($"The account type with {nameof(AccountDto.AccountTypeId)} [0] is invalid");
                },
                explanation: $"the {nameof(AccountDto.AccountTypeId)} property has an invalid value",
                useEntityModel: true
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheTheArchetypeIdIsMissing_ThenAValidationErrorShouldBeRaised()
        {
            await AssertValidationErrorOnAccountCreation(
                alterDto: dto => dto.ArchetypeId = 0, // Setup: AccountTypeId is set to an invalid value
                assertOverThrownException: e =>
                {
                    e.GetType().Should().Be<ClientModelAggregateException>();
                    var aggregateException = (ClientModelAggregateException)e;

                    aggregateException.InnerExceptions.Count.Should().Be(1);
                    aggregateException.InnerExceptions[0].Message.Should().Be($"The archetype with {nameof(AccountDto.ArchetypeId)} [0] is invalid");
                },
                explanation: $"the {nameof(AccountDto.ArchetypeId)} property has an invalid value",
                useEntityModel: true
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheTheSalesforceAccountIdIsLongerThan40_ThenAValidationErrorShouldBeRaised()
        {
            var memberInfo = typeof(AccountDto).GetProperty(nameof(AccountDto.SalesforceAccountId));
            Assert.IsNotNull(memberInfo);

            var attribute = (StringLengthAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(StringLengthAttribute));
            var length = attribute.MaximumLength;
            var min = attribute.MinimumLength;

            var minStringMessage = min > 0 ? $" a minimum length of {min} and" : "";

            await AssertValidationErrorOnAccountCreation(
                alterDto: dto => dto.SalesforceAccountId = new string('e', length + 1), // Setup: 1 SalesforceAccountId is just over 40 characters
                assertOverThrownException: e =>
                {
                    e.GetType().Should().Be<ClientModelAggregateException>();
                    var aggregateException = (ClientModelAggregateException)e;

                    aggregateException.InnerExceptions.Count.Should().Be(1);
                    aggregateException.InnerExceptions[0].Message.Should().Be($"The field {nameof(AccountDto.SalesforceAccountId)} must be a string with{minStringMessage} a maximum length of {length}.");
                },
                explanation: $"the {nameof(AccountDto.SalesforceAccountId)} property is larger than {length} characters"
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheTheSalesforceAccountNumberIsLongerThan40_ThenAValidationErrorShouldBeRaised()
        {
            var memberInfo = typeof(AccountDto).GetProperty(nameof(AccountDto.SalesforceAccountNumber));
            Assert.IsNotNull(memberInfo);

            var attribute = (StringLengthAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(StringLengthAttribute));
            var length = attribute.MaximumLength;
            var min = attribute.MinimumLength;

            var minStringMessage = min > 0 ? $" a minimum length of {min} and" : "";

            await AssertValidationErrorOnAccountCreation(
                alterDto: dto => dto.SalesforceAccountNumber = new string('e', length + 1), // Setup: 1 SalesforceAccountNumber is just over 40 characters
                assertOverThrownException: e =>
                {
                    e.GetType().Should().Be<ClientModelAggregateException>();
                    var aggregateException = (ClientModelAggregateException)e;

                    aggregateException.InnerExceptions.Count.Should().Be(1);
                    aggregateException.InnerExceptions[0].Message.Should().Be($"The field {nameof(AccountDto.SalesforceAccountNumber)} must be a string with{minStringMessage} a maximum length of {length}.");
                },
                explanation: $"the {nameof(AccountDto.SalesforceAccountNumber)} property is larger than {length} characters"
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheTheSalesforceAccountManagerIsLongerThan120_ThenAValidationErrorShouldBeRaised()
        {
            var memberInfo = typeof(AccountDto).GetProperty(nameof(AccountDto.SalesforceAccountManager));
            Assert.IsNotNull(memberInfo);

            var attribute = (StringLengthAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(StringLengthAttribute));
            var length = attribute.MaximumLength;
            var min = attribute.MinimumLength;

            var minStringMessage = min > 0 ? $" a minimum length of {min} and" : "";

            await AssertValidationErrorOnAccountCreation(
                alterDto: dto => dto.SalesforceAccountManager = new string('e', length + 1), // Setup: 1 SalesforceAccountManager is just over 120 characters
                assertOverThrownException: e =>
                {
                    e.GetType().Should().Be<ClientModelAggregateException>();
                    var aggregateException = (ClientModelAggregateException)e;

                    aggregateException.InnerExceptions.Count.Should().Be(1);
                    aggregateException.InnerExceptions[0].Message.Should().Be($"The field {nameof(AccountDto.SalesforceAccountManager)} must be a string with{minStringMessage} a maximum length of {length}.");
                },
                explanation: $"the {nameof(AccountDto.SalesforceAccountManager)} property is larger than {length} characters"
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheTheContractNumberIsLongerThan40_ThenAValidationErrorShouldBeRaised()
        {
            var memberInfo = typeof(AccountDto).GetProperty(nameof(AccountDto.ContractNumber));
            Assert.IsNotNull(memberInfo);

            var attribute = (StringLengthAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(StringLengthAttribute));
            var length = attribute.MaximumLength;
            var min = attribute.MinimumLength;

            var minStringMessage = min > 0 ? $" a minimum length of {min} and" : "";

            await AssertValidationErrorOnAccountCreation(
                alterDto: dto => dto.ContractNumber = new string('e', length + 1), // Setup: 1 ContractNumber is just over 40 characters
                assertOverThrownException: e =>
                {
                    e.GetType().Should().Be<ClientModelAggregateException>();
                    var aggregateException = (ClientModelAggregateException)e;

                    aggregateException.InnerExceptions.Count.Should().Be(1);
                    aggregateException.InnerExceptions[0].Message.Should().Be($"The field {nameof(AccountDto.ContractNumber)} must be a string with{minStringMessage} a maximum length of {length}.");
                },
                explanation: $"the {nameof(AccountDto.ContractNumber)} property is larger than {length} characters"
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheTheIdentityProviderNameIsMissing_ThenAValidationErrorShouldBeRaised()
        {
            await AssertValidationErrorOnAccountCreation(
                alterDto: dto => dto.IdentityProviders.First().Name = null, // Setup: 1 IdentityProviders[*].Name is null
                assertOverThrownException: e =>
                {
                    e.GetType().Should().Be<ClientModelAggregateException>();
                    var aggregateException = (ClientModelAggregateException)e;

                    aggregateException.InnerExceptions.Count.Should().Be(1);
                    aggregateException.InnerExceptions[0].Message.Should().Be($"The {nameof(IdentityProviderDto.Name)} field is required.");
                },
                explanation: $"the {nameof(IdentityProviderDto.Name)} property is null"
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheTheIdentityProviderNameIsLongerThan120_ThenAValidationErrorShouldBeRaised()
        {
            var memberInfo = typeof(IdentityProviderDto).GetProperty(nameof(IdentityProviderDto.Name));
            Assert.IsNotNull(memberInfo);

            var attribute = (StringLengthAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(StringLengthAttribute));
            var length = attribute.MaximumLength;
            var min = attribute.MinimumLength;

            var minStringMessage = min > 0 ? $" a minimum length of {min} and" : "";

            await AssertValidationErrorOnAccountCreation(
                alterDto: dto => dto.IdentityProviders.First().Name = new string('e', length + 1), // Setup: 1 IdentityProviders[*].Name is just over 120 characters
                assertOverThrownException: e =>
                {
                    e.GetType().Should().Be<ClientModelAggregateException>();
                    var aggregateException = (ClientModelAggregateException)e;

                    aggregateException.InnerExceptions.Count.Should().Be(1);
                    aggregateException.InnerExceptions[0].Message.Should().Be($"The field {nameof(IdentityProviderDto.Name)} must be a string with{minStringMessage} a maximum length of {length}.");
                },
                explanation: $"the {nameof(AccountDto.ContractNumber)} property is larger than {length} characters"
            );
        }
    }
}
