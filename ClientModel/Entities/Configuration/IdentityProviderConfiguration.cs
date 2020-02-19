using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClientModel.Entities.Configuration
{
    public class IdentityProviderConfiguration : IEntityTypeConfiguration<IdentityProvider>
    {
        public void Configure(EntityTypeBuilder<IdentityProvider> builder)
        {
            builder.ToTable("IdentityProvider", "Client");

            builder.Property(e => e.IdentityProviderId).HasDefaultValueSql("next value for Client.IdentityProviderId");

            builder.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(128)
                .HasDefaultValueSql("(suser_sname())");

            builder.Property(e => e.CreatedDate)
                .HasColumnType("datetime2(2)")
                .HasDefaultValueSql("(getutcdate())");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(120)
                .IsUnicode(false);

            builder.HasOne(d => d.Account)
                .WithMany(p => p.IdentityProviders)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("FK_IdentityProvider_Account");
        }
    }
}
