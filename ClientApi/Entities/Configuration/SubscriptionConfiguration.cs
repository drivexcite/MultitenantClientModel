using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClientApi.Entities.Configuration
{
    public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
    {
        public void Configure(EntityTypeBuilder<Subscription> builder)
        {
            builder.ToTable("Subscription", "Client");

            builder.HasIndex(e => e.AccountId)
                .HasName("IX_Subscription_Account");

            builder.HasIndex(e => e.SubscriptionTypeId);

            builder.Property(e => e.SubscriptionId).HasDefaultValueSql("next value for Client.SubscriptionId");

            builder.Property(e => e.ActivationDate).HasColumnType("datetime2(2)");

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
                .HasMaxLength(120);

            builder.Property(e => e.OrganizationalUnit).HasMaxLength(120);

            builder.Property(e => e.Tags).HasDefaultValueSql("('[]')");

            builder.HasOne(d => d.Account)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(d => d.AccountId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Subscription_Account");

            builder.HasOne(d => d.SubscriptionType)
                .WithMany()
                .HasForeignKey(d => d.SubscriptionTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Subscription_SubscriptionType");
        }
    }
}
