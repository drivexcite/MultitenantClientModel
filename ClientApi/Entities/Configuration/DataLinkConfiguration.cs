using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClientApi.Entities.Configuration
{
    public class DataLinkConfiguration : IEntityTypeConfiguration<DataLink>
    {
        public void Configure(EntityTypeBuilder<DataLink> builder)
        {
            builder.HasKey(e => new { e.FromSubscriptionId, e.ToSubscriptionId, e.TypeDataLinkTypeId });

            builder.ToTable("DataLink", "Client");

            builder.HasIndex(e => e.FromSubscriptionId)
                .HasName("IX_DataLink_FromSubscription");

            builder.HasIndex(e => e.ToSubscriptionId);

            builder.HasIndex(e => e.TypeDataLinkTypeId)
                .HasName("IX_DataLink_DataLinkType");

            builder.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(128)
                .HasDefaultValueSql("(suser_sname())");

            builder.Property(e => e.CreatedDate)
                .HasColumnType("datetime2(2)")
                .HasDefaultValueSql("(getutcdate())");

            builder.HasOne(d => d.From)
                .WithMany()
                .HasForeignKey(d => d.FromSubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DataLink_FromSubscription");

            builder.HasOne(d => d.To)
                .WithMany()
                .HasForeignKey(d => d.ToSubscriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DataLink_ToSubscription");

            builder.HasOne(d => d.Type)
                .WithMany()
                .HasForeignKey(d => d.TypeDataLinkTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DataLink_DataLinkType");
        }
    }
}
