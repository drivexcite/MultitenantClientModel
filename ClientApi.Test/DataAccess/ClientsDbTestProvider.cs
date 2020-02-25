using System;
using ClientModel.Entities;
using Microsoft.EntityFrameworkCore;

namespace ClientModel.Test.DataAccess
{
    public class ClientsDbTestProvider
    {
        public static ClientsDb CreateInMemoryClientDb()
        {
            var options = new DbContextOptionsBuilder<ClientsDb>()
                .UseInMemoryDatabase(databaseName: $"InMemory:{DateTime.Now.GetHashCode()}{DateTime.UtcNow.Millisecond}")
                .Options;

            var db = new ClientsDb(options);

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
            return db;
        }
    }
}
