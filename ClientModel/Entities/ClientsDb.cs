using System;
using ClientModel.Entities.Configuration;
using Microsoft.EntityFrameworkCore;

namespace ClientModel.Entities
{
    // Scaffold-DbContext "Server=(local);Database=Clients;Trusted_Connection=True;" Microsoft.EntityFrameworkCore.SqlServer -OutputDir Entities -Context ClientsDb
    // https://docs.microsoft.com/en-us/ef/core/miscellaneous/cli/powershell
    public partial class ClientsDb : DbContext
    {
        public ClientsDb()
        {
        }

        public ClientsDb(DbContextOptions<ClientsDb> options)
            : base(options)
        {
        }

        public virtual DbSet<Account> Accounts { get; set; }
        public virtual DbSet<AccountType> AccountTypes { get; set; }
        public virtual DbSet<Archetype> Archetypes { get; set; }
        public virtual DbSet<DataLink> DataLinks { get; set; }
        public virtual DbSet<DataLinkType> DataLinkTypes { get; set; }
        public virtual DbSet<IdentityProvider> IdentityProviders { get; set; }
        public virtual DbSet<IdentityProviderMapping> IdentityProviderMappings { get; set; }
        public virtual DbSet<Subscription> Subscriptions { get; set; }
        public virtual DbSet<SubscriptionType> SubscriptionTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new AccountConfiguration());
            modelBuilder.ApplyConfiguration(new AccountTypeConfiguration());
            modelBuilder.ApplyConfiguration(new ArchetypeConfiguration());
            modelBuilder.ApplyConfiguration(new DataLinkConfiguration());
            modelBuilder.ApplyConfiguration(new DataLinkTypeConfiguration());
            modelBuilder.ApplyConfiguration(new IdentityProviderConfiguration());
            modelBuilder.ApplyConfiguration(new IdentityProviderMappingConfiguration());
            modelBuilder.ApplyConfiguration(new SubscriptionConfiguration());
            modelBuilder.ApplyConfiguration(new SubscriptionTypeConfiguration());

            modelBuilder.HasSequence<int>("AccountId", "Client");
            modelBuilder.HasSequence<int>("IdentityProviderId", "Client");
            modelBuilder.HasSequence<int>("IdentitySourceId", "Client");
            modelBuilder.HasSequence<int>("PrimarySubscriptionId", "Client");
            modelBuilder.HasSequence<int>("SubscriptionId", "Client");

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
