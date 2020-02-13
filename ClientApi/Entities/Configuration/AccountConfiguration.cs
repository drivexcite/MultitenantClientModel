using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClientApi.Entities.Configuration
{
    public class AccountConfiguration : IEntityTypeConfiguration<Account>
    {
        public void Configure(EntityTypeBuilder<Account> builder)
        {
            builder.ToTable("Account", "Client");

            builder.HasIndex(e => e.AccountTypeId);

            builder.HasIndex(e => e.ArchetypeId)
                .HasName("IX_Account_ClientArchetype");

            builder.HasIndex(e => e.Name)
                .HasName("UQ_Account_Name")
                .IsUnique();

            builder.Property(e => e.AccountId).HasDefaultValueSql("next value for Client.AccountId");

            builder.Property(e => e.ContractNumber)
                .HasMaxLength(40)
                .IsUnicode(false);

            builder.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(128)
                .HasDefaultValueSql("(suser_sname())");

            builder.Property(e => e.CreatedDate)
                .HasColumnType("datetime2(2)")
                .HasDefaultValueSql("(getutcdate())");

            builder.Property(e => e.Enabled)
                .IsRequired()
                .HasDefaultValueSql("((1))");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(120)
                .IsUnicode(false);

            builder.Property(e => e.SalesforceAccountId)
                .IsRequired()
                .HasMaxLength(40)
                .IsUnicode(false);

            builder.Property(e => e.SalesforceAccountManager).HasMaxLength(120);

            builder.Property(e => e.SalesforceAccountNumber)
                .HasMaxLength(40)
                .IsUnicode(false);

            builder.Property(e => e.SalesforceAccountUrl)
                .HasMaxLength(40)
                .IsUnicode(false);

            builder.HasOne(d => d.AccountType)
                .WithMany()
                .HasForeignKey(d => d.AccountTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_AccountType");

            builder.HasOne(d => d.Archetype)
                .WithMany()
                .HasForeignKey(d => d.ArchetypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_ClientArchetype");
        }
    }
}
