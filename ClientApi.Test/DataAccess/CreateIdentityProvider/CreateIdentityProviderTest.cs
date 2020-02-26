using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClientModel.DataAccess.Create.CreateAccount;
using ClientModel.DataAccess.Create.CreateIdentityProvider;
using ClientModel.DataAccess.Create.CreateSubscription;
using ClientModel.Dtos;
using ClientModel.Exceptions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientModel.Test.DataAccess.CreateIdentityProvider
{
    [TestClass]
    public class CreateIdentityProviderTest
    {
        static IMapper Mapper { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Mapper = ClientModelMapperProvider.CreateAutoMapper();
        }

        [TestMethod]
        public async Task GivenAValidIdPDefinition_WhenICallTheCreateDelegate_ThenTheProviderShouldBePersisted()
        {
            // Setup: Create a database with an Account and two existing Subscriptions.
            await using var db = ClientsDbTestProvider.CreateInMemoryClientDb();
            var account = DtoProvider.CreateValidAccountDefinition();

            var createAccountDelegate = new CreateAccountDelegate(db, Mapper);
            account = await createAccountDelegate.CreateAccountAsync(account);

            var identityProvider = new IdentityProviderDto { Name = "Health Dialog Secondary IdP" };

            // System under test: CreateIdentityProviderDelegate
            var createIdentityProvider = new CreateIdentityProviderDelegate(db, Mapper);

            // Exercise: invoke Create Identity Provider
            identityProvider = await createIdentityProvider.CreateIdentityProvider(account.AccountId, identityProvider);

            // Assert: the number of Accounts in the InMemory database is still 1
            // Assert: the number of Subscriptions in the InMemory database is still 2
            // Assert: the number of Subscriptions in the InMemory database is now 2
            db.Accounts.Count().Should().Be(1, $"The number of {nameof(db.Accounts)} in the ClientsDb is not 1");
            db.Subscriptions.Count().Should().Be(2, $"The number of {nameof(db.Subscriptions)} in the ClientsDb is not 2");
            db.IdentityProviders.Count().Should().Be(2, $"The number of {nameof(db.IdentityProviders)} in the ClientsDb is not 2");

            identityProvider.IdentityProviderId.Should().NotBe(default);
            identityProvider.Name.Should().Be("Health Dialog Secondary IdP");
        }

        [TestMethod]
        public async Task GivenAnInvalidIdPDefinition_WhenTheNameIsMissing_ThenTheDelegateShouldRaiseAnError()
        {
            // System under test: CreateIdentityProviderDelegate
            var createIdentityProvider = new CreateIdentityProviderDelegate(null, Mapper);

            // Exercise: invoke Create Identity Provider
            try
            {
                await createIdentityProvider.CreateIdentityProvider(1, new IdentityProviderDto());
                Assert.Fail($"The invocation to {nameof(CreateIdentityProviderDelegate.CreateIdentityProvider)} should not succeed when  {nameof(IdentityProviderDto)}.{nameof(IdentityProviderDto.Name)} is missing.");
            }
            catch (Exception e)
            {
                e.GetType().Should().Be<ClientModelAggregateException>();
                var aggregateException = (ClientModelAggregateException)e;

                aggregateException.InnerExceptions.Count.Should().Be(1);
                aggregateException.InnerExceptions[0].Message.Should().Be($"The {nameof(IdentityProviderDto.Name)} field is required.");
            }
        }

        [TestMethod]
        public async Task GivenAValidIdPDefinition_WhenTheAccountIdIsInvalid_ThenTheDelegateShouldRaiseAnError()
        {
            await using var db = ClientsDbTestProvider.CreateInMemoryClientDb();

            // System under test: CreateIdentityProviderDelegate
            var createIdentityProvider = new CreateIdentityProviderDelegate(db, Mapper);

            // Exercise: invoke Create Identity Provider
            try
            {
                await createIdentityProvider.CreateIdentityProvider(-1, new IdentityProviderDto { Name = "Unimportant" });
                Assert.Fail($"The invocation to {nameof(CreateIdentityProviderDelegate.CreateIdentityProvider)} should not succeed when ${nameof(AccountDto.AccountId)} is invalid.");
            }
            catch (Exception e)
            {
                e.GetType().Should().Be<AccountNotFoundException>();
                e.Message.Should().Be($"An account with {nameof(AccountDto.AccountId)} = -1 could not be found");
            }
        }

        [TestMethod]
        public async Task GivenAValidIdPDefinition_WhenICallTheCreateDelegateASecondTime_ThenTheDelegateShouldRaiseAnError()
        {
            // Setup: Create a database with an Account and two existing Subscriptions.
            await using var db = ClientsDbTestProvider.CreateInMemoryClientDb();
            var account = DtoProvider.CreateValidAccountDefinition();

            var createAccountDelegate = new CreateAccountDelegate(db, Mapper);
            account = await createAccountDelegate.CreateAccountAsync(account);

            // System under test: CreateIdentityProviderDelegate
            var createIdentityProvider = new CreateIdentityProviderDelegate(db, Mapper);

            // Exercise: invoke Create Identity Provider
            try
            {
                await createIdentityProvider.CreateIdentityProvider(account.AccountId, new IdentityProviderDto{ Name = "Duplicate" });
                await createIdentityProvider.CreateIdentityProvider(account.AccountId, new IdentityProviderDto { Name = "Duplicate" });

                Assert.Fail($"The invocation to {nameof(CreateIdentityProviderDelegate.CreateIdentityProvider)} should not succeed when  {nameof(IdentityProviderDto)}.{nameof(IdentityProviderDto.Name)} is missing.");
            }
            catch (Exception e)
            {
                e.GetType().Should().Be<MalformedAccountException>();
                e.Message.Should().Be($"An identity provider with the same name [Duplicate] already exists.");
            }

            // Assert: the number of Accounts in the InMemory database is still 1
            // Assert: the number of Subscriptions in the InMemory database is still 2
            // Assert: the number of Subscriptions in the InMemory database is now 2
            db.Accounts.Count().Should().Be(1, $"The number of {nameof(db.Accounts)} in the ClientsDb is not 1");
            db.Subscriptions.Count().Should().Be(2, $"The number of {nameof(db.Subscriptions)} in the ClientsDb is not 2");
            db.IdentityProviders.Count().Should().Be(2, $"The number of {nameof(db.IdentityProviders)} in the ClientsDb is not 2");
        }
    }
}
