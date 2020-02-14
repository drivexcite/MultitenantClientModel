using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClientModel.Entities.Configuration
{
    public class IdentityProviderMappingConfiguration : IEntityTypeConfiguration<IdentityProviderMapping>
    {
        public void Configure(EntityTypeBuilder<IdentityProviderMapping> builder)
        {
            builder.HasKey(e => new { e.SubscriptionId, e.IdentityProviderId });

            builder.ToTable("IdentityProviderMapping", "Client");

            builder.HasIndex(e => e.IdentityProviderId)
                .HasName("IX_IdentityProviderMapping_IdentityProvider");

            builder.HasIndex(e => e.SubscriptionId)
                .HasName("IX_IdentityProviderMapping_Subscription");

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

            builder.HasOne(d => d.IdentityProvider)
                .WithMany()
                .HasForeignKey(d => d.IdentityProviderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IdentityProviderMapping_IdentityProvider");

            builder.HasOne(d => d.Subscription)
                .WithMany(p => p.IdentityProviders)
                .HasForeignKey(d => d.SubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_IdentityProviderMapping_Subscription");
        }
    }
}
