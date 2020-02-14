using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClientModel.Entities.Configuration
{
    public class AccountTypeConfiguration : IEntityTypeConfiguration<AccountType>
    {
        public void Configure(EntityTypeBuilder<AccountType> builder)
        {
            builder.ToTable("AccountType", "Client");

            builder.HasIndex(e => e.Name)
                .HasName("UQ_AccountType_Name")
                .IsUnique();

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
