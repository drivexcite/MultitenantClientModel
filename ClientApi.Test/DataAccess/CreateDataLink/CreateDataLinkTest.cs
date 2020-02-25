using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using ClientModel.DataAccess.Create.CreateAccount;
using ClientModel.DataAccess.Create.CreateDataLink;
using ClientModel.Dtos;
using ClientModel.Exceptions;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ClientModel.Test.DataAccess.CreateDataLink
{
    [TestClass]
    public class CreateDataLinkTest
    {
        static IMapper Mapper { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            Mapper = ClientModelMapperProvider.CreateAutoMapper();
        }

        [TestMethod]
        public async Task GivenAnAccountDefinition_WhenICreateADataLinkBetweenTwoSubscriptions_ThenTheDataLinkShouldBePersisted()
        {
            // Setup:
            await using var db = ClientsDbTestProvider.CreateInMemoryClientDb();

            var account = DtoProvider.CreateValidAccountDefinition();
            var createAccountDelegate = new CreateAccountDelegate(db, Mapper);
            
            
            account = await createAccountDelegate.CreateAccountAsync(account);
            var dataLink = new DataLinkDto
            {
                FromSubscriptionId = 2, // Staging
                ToSubscriptionId = 1, // Production
                DataLinkTypeId = 1 // Customization
            };

            // System under test: CreateDataLinkDelegate
            var createDataLinkDelegate = new CreateDataLinkDelegate(db, Mapper);

            // Exercise: invoke CreateDataLink
            var createdDataLink = await createDataLinkDelegate.CreateDataLinkAsync(account.AccountId, dataLink);


            // Assert
            createdDataLink.From.Should().NotBeNullOrEmpty();
            createdDataLink.To.Should().NotBeNullOrEmpty();
            createdDataLink.Type.Should().NotBeNullOrEmpty().And.Be("Customization");

            db.DataLinks.Count().Should().Be(1);
        }

        [TestMethod]
        public async Task GivenADataLinkOperation_WhenIPassAnInvalidAccount_ThenTheDelegateShouldRaiseAnError()
        {
            // Setup:
            await using var db = ClientsDbTestProvider.CreateInMemoryClientDb();

            var account = DtoProvider.CreateValidAccountDefinition();
            var createAccountDelegate = new CreateAccountDelegate(db, Mapper);

            account = await createAccountDelegate.CreateAccountAsync(account);
            var dataLink = new DataLinkDto
            {
                FromSubscriptionId = 2, // Staging
                ToSubscriptionId = 1, // Production
                DataLinkTypeId = 1 // Customization
            };

            // System under test: CreateDataLinkDelegate
            var createDataLinkDelegate = new CreateDataLinkDelegate(db, Mapper);

            // Exercise: invoke CreateDataLink twice
            try
            {
                await createDataLinkDelegate.CreateDataLinkAsync(-1, dataLink);
                Assert.Fail($"An invocation to {nameof(CreateDataLinkDelegate.CreateDataLinkAsync)} should not have completed with an invalid {nameof(AccountDto.AccountId)}.");
            }
            catch (Exception e)
            {
                // Assert

                // Exception is the right type
                e.GetType().Should().Be<AccountNotFoundException>();
                e.Message.Should().Be($"An account with AccountId = -1 doesn't exist.");

                // Nothing persisted
                db.DataLinks.Count().Should().Be(0);
            }
        }

        [TestMethod]
        public async Task GivenADataLinkOperation_WhenICreateAnExistingDataLink_ThenTheDelegateShouldRaiseAnError()
        {
            // Setup:
            await using var db = ClientsDbTestProvider.CreateInMemoryClientDb();

            var account = DtoProvider.CreateValidAccountDefinition();
            var createAccountDelegate = new CreateAccountDelegate(db, Mapper);


            account = await createAccountDelegate.CreateAccountAsync(account);
            var dataLink = new DataLinkDto
            {
                FromSubscriptionId = 2, // Staging
                ToSubscriptionId = 1, // Production
                DataLinkTypeId = 1 // Customization
            };

            // System under test: CreateDataLinkDelegate
            var createDataLinkDelegate = new CreateDataLinkDelegate(db, Mapper);

            // Exercise: invoke CreateDataLink twice
            try
            {
                await createDataLinkDelegate.CreateDataLinkAsync(account.AccountId, dataLink);
                await createDataLinkDelegate.CreateDataLinkAsync(account.AccountId, dataLink);

                Assert.Fail($"An invocation to {nameof(CreateDataLinkDelegate.CreateDataLinkAsync)} should not have completed when a duplicate data link.");
            }
            catch (Exception e)
            {
                // Assert

                // Exception is the right type
                e.GetType().Should().Be<MalformedDataLinkException>();
                e.Message.Should().Be($"An existing DataLink from subscription with SubscriptionId = 2 to subscription with SubscriptionId = 1 already exists.");

                // Only one data link is persisted.
                db.DataLinks.Count().Should().Be(1);
            }
        }

        [TestMethod]
        public async Task GivenADataLinkOperation_WhenIPassAnInvalidDataLinkType_ThenTheDelegateShouldRaiseAnError()
        {
            // Setup:
            await using var db = ClientsDbTestProvider.CreateInMemoryClientDb();

            var account = DtoProvider.CreateValidAccountDefinition();
            var createAccountDelegate = new CreateAccountDelegate(db, Mapper);

            account = await createAccountDelegate.CreateAccountAsync(account);
            var dataLink = new DataLinkDto
            {
                FromSubscriptionId = 2, // Staging
                ToSubscriptionId = 1, // Production
                DataLinkTypeId = 128 // Invalid
            };

            // System under test: CreateDataLinkDelegate
            var createDataLinkDelegate = new CreateDataLinkDelegate(db, Mapper);

            // Exercise: invoke CreateDataLink with a reference to an invalid DataLinkTypeId
            try
            {
                await createDataLinkDelegate.CreateDataLinkAsync(account.AccountId, dataLink);
                Assert.Fail($"An invocation to {nameof(CreateDataLinkDelegate.CreateDataLinkAsync)} should not have completed with an invalid {nameof(DataLinkDto.DataLinkTypeId)}");
            }
            catch (Exception e)
            {
                // Assert

                // Exception is the right type
                e.GetType().Should().Be<DataLinkTypeNotFoundException>();
                e.Message.Should().Be($"A data link type with DataLinkTypeId = 128 could not be found.");

                // Nothing persisted
                db.DataLinks.Count().Should().Be(0);
            }
        }

        [TestMethod]
        public async Task GivenADataLinkOperation_WhenIPassAnInvalidFromSubscriptionId_ThenTheDelegateShouldRaiseAnError()
        {
            // Setup:
            await using var db = ClientsDbTestProvider.CreateInMemoryClientDb();

            var account = DtoProvider.CreateValidAccountDefinition();
            var createAccountDelegate = new CreateAccountDelegate(db, Mapper);

            account = await createAccountDelegate.CreateAccountAsync(account);
            var dataLink = new DataLinkDto
            {
                FromSubscriptionId = -1, // Invalid
                ToSubscriptionId = 2, // Production
                DataLinkTypeId = 1 // Customization
            };

            // System under test: CreateDataLinkDelegate
            var createDataLinkDelegate = new CreateDataLinkDelegate(db, Mapper);

            // Exercise: invoke CreateDataLink with an invalid FromSubscriptionId
            try
            {
                await createDataLinkDelegate.CreateDataLinkAsync(account.AccountId, dataLink);
                Assert.Fail($"An invocation to {nameof(CreateDataLinkDelegate.CreateDataLinkAsync)} should not have completed with an invalid {nameof(DataLinkDto.FromSubscriptionId)}");
            }
            catch (Exception e)
            {
                // Assert

                // Exception is the right type
                e.GetType().Should().Be<MalformedSubscriptionException>();
                e.Message.Should().Be($"A subscription with SubscriptionId = -1 does not exists inside Account with AccountId {account.AccountId}");

                // Nothing persisted
                db.DataLinks.Count().Should().Be(0);
            }
        }

        [TestMethod]
        public async Task GivenADataLinkOperation_WhenIPassAnInvalidToSubscriptionId_ThenTheDelegateShouldRaiseAnError()
        {
            // Setup:
            await using var db = ClientsDbTestProvider.CreateInMemoryClientDb();

            var account = DtoProvider.CreateValidAccountDefinition();
            var createAccountDelegate = new CreateAccountDelegate(db, Mapper);

            account = await createAccountDelegate.CreateAccountAsync(account);
            var dataLink = new DataLinkDto
            {
                FromSubscriptionId = 1, // Staging
                ToSubscriptionId = -1, // Invalid
                DataLinkTypeId = 1 // Customization
            };

            // System under test: CreateDataLinkDelegate
            var createDataLinkDelegate = new CreateDataLinkDelegate(db, Mapper);

            // Exercise: invoke CreateDataLink with a reference to an invalid ToSubscriptionId
            try
            {
                await createDataLinkDelegate.CreateDataLinkAsync(account.AccountId, dataLink);
                Assert.Fail($"An invocation to {nameof(CreateDataLinkDelegate.CreateDataLinkAsync)} should not have completed with an invalid {nameof(DataLinkDto.FromSubscriptionId)}");
            }
            catch (Exception e)
            {
                // Assert

                // Exception is the right type
                e.GetType().Should().Be<MalformedSubscriptionException>();
                e.Message.Should().Be($"A subscription with SubscriptionId = -1 does not exists inside Account with AccountId {account.AccountId}");

                // Nothing persisted
                db.DataLinks.Count().Should().Be(0);
            }
        }
    }
}
