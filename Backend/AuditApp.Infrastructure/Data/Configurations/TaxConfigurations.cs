using AuditApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditApp.Infrastructure.Data.Configurations;

public class TaxFilingConfiguration : IEntityTypeConfiguration<TaxFiling>
{
    public void Configure(EntityTypeBuilder<TaxFiling> builder)
    {
        builder.ToTable("tax_filings");
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.FilingCode).HasMaxLength(20).IsRequired();
        builder.Property(e => e.TaxType).HasMaxLength(10).HasConversion<string>().IsRequired();
        builder.Property(e => e.ClientId);
        builder.Property(e => e.ClientName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.BranchId);
        builder.Property(e => e.FilingDate).IsRequired();
        builder.Property(e => e.TaxNumber).HasMaxLength(20).IsRequired();
        builder.Property(e => e.PeriodNumber).HasMaxLength(20);
        builder.Property(e => e.PeriodType).HasMaxLength(10).HasConversion<string>();
        builder.Property(e => e.PaymentStatus).HasMaxLength(20);
        builder.Property(e => e.Notes);
        builder.Property(e => e.CreatedBy);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.IsDeleted);

        builder.HasIndex(e => e.FilingCode).IsUnique();
        builder.HasIndex(e => e.ClientId);
        builder.HasIndex(e => e.TaxType);
        builder.HasIndex(e => e.BranchId);
        builder.HasIndex(e => e.FilingDate);
        builder.HasIndex(e => e.TaxNumber);

        builder.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId);
        builder.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId);
    }
}
