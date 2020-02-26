using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClientModel.DataAccess.Create.CreateAccount;
using ClientModel.DataAccess.Create.CreateSubscription;
using ClientModel.Dtos;
using ClientModel.Exceptions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientModel.Test.DataAccess.CreateSubscription
{
    [TestClass]
    public class CreateSubscriptionTest
    {
        static IMapper Mapper { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Mapper = ClientModelMapperProvider.CreateAutoMapper();
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheSubscriptionIsValid_ThenTheSubscriptionShouldBePersisted()
        {
            // Setup: Create a database with an Account and two existing Subscriptions.
            await using var db = ClientsDbTestProvider.CreateInMemoryClientDb();
            var account = DtoProvider.CreateValidAccountDefinition();

            var createAccountDelegate = new CreateAccountDelegate(db, Mapper);
            account = await createAccountDelegate.CreateAccountAsync(account);
            
            var subscription = DtoProvider.CreateValidSubscriptionDefinition();

            // System under test: CreateSubscriptionDelegate
            var createSubscription = new CreateSubscriptionDelegate(db, Mapper);
            
            // Exercise: invoke CreateSubscription
            subscription = await createSubscription.CreateSubscriptionAsync(account.AccountId, subscription);

            // Assert: the number of Accounts in the InMemory database is still 1
            // Assert: the number of Subscriptions in the InMemory database is now 3
            db.Accounts.Count().Should().Be(1, $"The number of {nameof(db.Accounts)} in the ClientsDb is not 1");
            db.Subscriptions.Count().Should().Be(3, $"The number of {nameof(db.Subscriptions)} in the ClientsDb is not 3");
            
            subscription.SubscriptionId.Should().NotBe(default, $"The underlying provider should generate a value for {nameof(SubscriptionDto.SubscriptionId)}");
            subscription.SubscriptionName.Should().Be("Health Dialog - Demo");
            subscription.Description.Should().Be("Demo subscription for Health Dialog");
            subscription.Tags.Count.Should().Be(3);
            subscription.OrganizationalUnit.Should().Be("Demo");
            subscription.SubscriptionTypeId.Should().Be(3);
        }

        private async Task AssertValidationErrorOnAccountCreation(Action<AccountDto, SubscriptionDto> alterDto, Action<Exception> assertOverThrownException, string explanation)
        {
            // Setup
            await using var db = ClientsDbTestProvider.CreateInMemoryClientDb();
            var account = DtoProvider.CreateValidAccountDefinition();

            var createAccountDelegate = new CreateAccountDelegate(db, Mapper);
            account = await createAccountDelegate.CreateAccountAsync(account);

            var subscription = DtoProvider.CreateValidSubscriptionDefinition();
            alterDto(account, subscription);

            // System under test: CreateSubscriptionDelegate
            var createSubscription = new CreateSubscriptionDelegate(db, Mapper);

            // Exercise: invoke CreateSubscription
            try
            {
                await createSubscription.CreateSubscriptionAsync(account.AccountId, subscription);
                Assert.Fail($"The invocation to {nameof(CreateSubscriptionDelegate.CreateSubscriptionAsync)} should not succeed when {explanation}.");
            }
            catch (Exception e)
            {
                assertOverThrownException(e);
            }
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheSubscriptionDoesntHaveAName_ThenTheDelegateShouldRaiseAnError()
        {
            await AssertValidationErrorOnAccountCreation(
                alterDto: (account, subscription) => subscription.SubscriptionName = "", // Setup: Subscription Name is empty
                assertOverThrownException: e =>
                {
                    e.GetType().Should().Be<ClientModelAggregateException>();
                    var aggregateException = (ClientModelAggregateException)e;

                    aggregateException.InnerExceptions.Count.Should().Be(1);
                    aggregateException.InnerExceptions[0].Message.Should().Be($"The {nameof(SubscriptionDto.SubscriptionName)} field is required.");
                },
                explanation: $"the {nameof(SubscriptionDto.SubscriptionName)} property is missing"
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheSubscriptionNameIsLongerThan120_ThenTheDelegateShouldRaiseAnError()
        {
            var memberInfo = typeof(SubscriptionDto).GetProperty(nameof(SubscriptionDto.SubscriptionName));
            memberInfo.Should().NotBeNull();

            var attribute = (StringLengthAttribute)Attribute.GetCustomAttribute(memberInfo, typeof(StringLengthAttribute));
            var length = attribute.MaximumLength;
            var min = attribute.MinimumLength;

            await AssertValidationErrorOnAccountCreation(
                alterDto: (account, subscription) => subscription.SubscriptionName = new string('e', length + 1), // Setup: Subscription Name is longer than 120
                assertOverThrownException: e =>
                {
                    e.GetType().Should().Be<ClientModelAggregateException>();
                    var aggregateException = (ClientModelAggregateException)e;

                    aggregateException.InnerExceptions.Count.Should().Be(1);
                    aggregateException.InnerExceptions[0].Message.Should().Be($"The field {nameof(SubscriptionDto.SubscriptionName)} must be a string with a minimum length of {min} and a maximum length of {length}.");
                },
                explanation: $"the {nameof(SubscriptionDto.SubscriptionName)} property is larger than {length} characters"
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheSubscriptionTypeIdIsInvalid_ThenTheDelegateShouldRaiseAnError()
        {
            await AssertValidationErrorOnAccountCreation(
                alterDto: (account, subscription) => subscription.SubscriptionTypeId = 128, // Setup: SubscriptionTypeId is invalid
                assertOverThrownException: e =>
                {
                    e.GetType().Should().Be<MalformedSubscriptionException>();
                    e.Message.Should().Be($"A subscription type with {nameof(SubscriptionDto.SubscriptionTypeId)} = 128 doesn't exist.");
                },
                explanation: $"the {nameof(SubscriptionDto.SubscriptionTypeId)} property is invalid"
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheParentAccountIsInvalid_ThenTheDelegateShouldRaiseAnError()
        {
            await AssertValidationErrorOnAccountCreation(
                alterDto: (account, subscription) => account.AccountId = -1, // Setup: AccountId is invalid
                assertOverThrownException: e =>
                {
                    e.GetType().Should().Be<AccountNotFoundException>();
                    e.Message.Should().Be($"An account with {nameof(AccountDto.AccountId)} = -1 could not be found.");
                },
                explanation: $"the {nameof(AccountDto.AccountId)} parameter is invalid"
            );
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenTheSubscriptionIdAlreadyExists_ThenTheDelegateShouldRaiseAnError()
        {
            // Setup
            await using var db = ClientsDbTestProvider.CreateInMemoryClientDb();
            var account = DtoProvider.CreateValidAccountDefinition();

            var createAccountDelegate = new CreateAccountDelegate(db, Mapper);
            account = await createAccountDelegate.CreateAccountAsync(account);

            var subscription = DtoProvider.CreateValidSubscriptionDefinition();

            // System under test: CreateSubscriptionDelegate
            var createSubscription = new CreateSubscriptionDelegate(db, Mapper);
            subscription = await createSubscription.CreateSubscriptionAsync(account.AccountId, subscription);

            // Exercise: invoke CreateSubscription
            try
            {
                subscription = await createSubscription.CreateSubscriptionAsync(account.AccountId, subscription);
                Assert.Fail($"The invocation to {nameof(CreateSubscriptionDelegate.CreateSubscriptionAsync)} should not succeed a second time.");
            }
            catch (Exception e)
            {
                e.GetType().Should().Be<MalformedSubscriptionException>();
                e.Message.Should().Be($"An existing subscription with {nameof(SubscriptionDto.SubscriptionId)} = {subscription.SubscriptionId} already exists.");
            }
        }
    }
}
