using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ClientModel.Entities.Configuration
{
    public class SubscriptionTypeConfiguration : IEntityTypeConfiguration<SubscriptionType>
    {
        public void Configure(EntityTypeBuilder<SubscriptionType> builder)
        {
            builder.ToTable("SubscriptionType", "Client");

            builder.HasIndex(e => e.Name)
                .HasName("UQ_SubscriptionType_Name");

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
        }
    }
}
