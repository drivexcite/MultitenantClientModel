using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClientApi.Entities.Configuration
{
    public class DataLinkTypeConfiguration : IEntityTypeConfiguration<DataLinkType>
    {
        public void Configure(EntityTypeBuilder<DataLinkType> builder)
        {
            builder.ToTable("DataLinkType", "Client");

            builder.Property(e => e.CreatedBy)
                .IsRequired()
                .HasMaxLength(128)
                .HasDefaultValueSql("(suser_sname())");

            builder.Property(e => e.CreatedDate)
                .HasColumnType("datetime2(2)")
                .HasDefaultValueSql("(getutcdate())");

            builder.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(40)
                .IsUnicode(false);
        }
    }
}
