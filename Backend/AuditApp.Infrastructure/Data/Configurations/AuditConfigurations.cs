using AuditApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditApp.Infrastructure.Data.Configurations;

public class AuditAssuranceRecordConfiguration : IEntityTypeConfiguration<AuditAssuranceRecord>
{
    public void Configure(EntityTypeBuilder<AuditAssuranceRecord> builder)
    {
        builder.ToTable("audit_assurance_records");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RecordCode).HasMaxLength(20).IsRequired();
        builder.Property(e => e.RecordDate).IsRequired();
        builder.Property(e => e.ClientId);
        builder.Property(e => e.ClientName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.BranchId);
        builder.Property(e => e.ClientLogo).HasMaxLength(1000);
        builder.Property(e => e.PaymentStatus).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.Process).HasMaxLength(50);
        builder.Property(e => e.Assignment);
        builder.Property(e => e.SubTotal).HasPrecision(15, 2);
        builder.Property(e => e.Discount).HasPrecision(15, 2);
        builder.Property(e => e.TotalPayment).HasPrecision(15, 2);
        builder.Property(e => e.PartialAmount).HasPrecision(15, 2);
        builder.Property(e => e.PaymentOption).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.Notes);
        builder.Property(e => e.CreatedBy);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.IsDeleted);

        builder.HasIndex(e => e.RecordCode).IsUnique();
        builder.HasIndex(e => e.ClientId);
        builder.HasIndex(e => e.BranchId);
        builder.HasIndex(e => e.RecordDate);

        builder.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId);
        builder.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId);
        builder.HasOne(e => e.Creator).WithMany().HasForeignKey(e => e.CreatedBy);
    }
}

public class ForensicAuditRecordConfiguration : IEntityTypeConfiguration<ForensicAuditRecord>
{
    public void Configure(EntityTypeBuilder<ForensicAuditRecord> builder)
    {
        builder.ToTable("forensic_audit_records");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RecordCode).HasMaxLength(20).IsRequired();
        builder.Property(e => e.RecordDate).IsRequired();
        builder.Property(e => e.ClientId);
        builder.Property(e => e.ClientName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.BranchId);
        builder.Property(e => e.ClientLogo).HasMaxLength(1000);
        builder.Property(e => e.PaymentStatus).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.Process).HasMaxLength(50);
        builder.Property(e => e.Assignment);
        builder.Property(e => e.PeriodNumber).HasMaxLength(20);
        builder.Property(e => e.PeriodType).HasMaxLength(10).HasConversion<string>();
        builder.Property(e => e.SubTotal).HasPrecision(15, 2);
        builder.Property(e => e.Discount).HasPrecision(15, 2);
        builder.Property(e => e.TotalPayment).HasPrecision(15, 2);
        builder.Property(e => e.PartialAmount).HasPrecision(15, 2);
        builder.Property(e => e.PaymentOption).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.Notes);
        builder.Property(e => e.CreatedBy);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.IsDeleted);

        builder.HasIndex(e => e.RecordCode).IsUnique();
        builder.HasIndex(e => e.ClientId);
        builder.HasIndex(e => e.BranchId);
        builder.HasIndex(e => e.RecordDate);

        builder.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId);
        builder.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId);
        builder.HasOne(e => e.Creator).WithMany().HasForeignKey(e => e.CreatedBy);
    }
}

public class InternalAuditRecordConfiguration : IEntityTypeConfiguration<InternalAuditRecord>
{
    public void Configure(EntityTypeBuilder<InternalAuditRecord> builder)
    {
        builder.ToTable("internal_audit_records");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RecordCode).HasMaxLength(20).IsRequired();
        builder.Property(e => e.RecordDate).IsRequired();
        builder.Property(e => e.ClientId);
        builder.Property(e => e.ClientName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.BranchId);
        builder.Property(e => e.ClientLogo).HasMaxLength(1000);
        builder.Property(e => e.PaymentStatus).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.Process).HasMaxLength(50);
        builder.Property(e => e.Assignment);
        builder.Property(e => e.PeriodNumber).HasMaxLength(20);
        builder.Property(e => e.PeriodType).HasMaxLength(10).HasConversion<string>();
        builder.Property(e => e.SubTotal).HasPrecision(15, 2);
        builder.Property(e => e.Discount).HasPrecision(15, 2);
        builder.Property(e => e.TotalPayment).HasPrecision(15, 2);
        builder.Property(e => e.PartialAmount).HasPrecision(15, 2);
        builder.Property(e => e.PaymentOption).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.Notes);
        builder.Property(e => e.CreatedBy);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.IsDeleted);

        builder.HasIndex(e => e.RecordCode).IsUnique();
        builder.HasIndex(e => e.ClientId);
        builder.HasIndex(e => e.BranchId);
        builder.HasIndex(e => e.RecordDate);

        builder.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId);
        builder.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId);
        builder.HasOne(e => e.Creator).WithMany().HasForeignKey(e => e.CreatedBy);
    }
}

public class ManagementAccountRecordConfiguration : IEntityTypeConfiguration<ManagementAccountRecord>
{
    public void Configure(EntityTypeBuilder<ManagementAccountRecord> builder)
    {
        builder.ToTable("management_account_records");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RecordCode).HasMaxLength(20).IsRequired();
        builder.Property(e => e.RecordDate).IsRequired();
        builder.Property(e => e.ClientId);
        builder.Property(e => e.ClientName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.BranchId);
        builder.Property(e => e.ClientLogo).HasMaxLength(1000);
        builder.Property(e => e.PaymentStatus).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.Process).HasMaxLength(50);
        builder.Property(e => e.Assignment);
        builder.Property(e => e.SubTotal).HasPrecision(15, 2);
        builder.Property(e => e.Discount).HasPrecision(15, 2);
        builder.Property(e => e.TotalPayment).HasPrecision(15, 2);
        builder.Property(e => e.PartialAmount).HasPrecision(15, 2);
        builder.Property(e => e.PaymentOption).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.Notes);
        builder.Property(e => e.CreatedBy);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.IsDeleted);

        builder.HasIndex(e => e.RecordCode).IsUnique();
        builder.HasIndex(e => e.ClientId);
        builder.HasIndex(e => e.BranchId);
        builder.HasIndex(e => e.RecordDate);

        builder.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId);
        builder.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId);
        builder.HasOne(e => e.Creator).WithMany().HasForeignKey(e => e.CreatedBy);
    }
}

public class InternalControlRecordConfiguration : IEntityTypeConfiguration<InternalControlRecord>
{
    public void Configure(EntityTypeBuilder<InternalControlRecord> builder)
    {
        builder.ToTable("internal_control_records");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RecordCode).HasMaxLength(20).IsRequired();
        builder.Property(e => e.RecordDate).IsRequired();
        builder.Property(e => e.ClientId);
        builder.Property(e => e.ClientName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.BranchId);
        builder.Property(e => e.ClientLogo).HasMaxLength(1000);
        builder.Property(e => e.Assignment);
        builder.Property(e => e.PeriodNumber).HasMaxLength(20);
        builder.Property(e => e.PeriodType).HasMaxLength(10).HasConversion<string>();
        builder.Property(e => e.PaymentStatus).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.Process).HasMaxLength(50);
        builder.Property(e => e.SubTotal).HasPrecision(15, 2);
        builder.Property(e => e.Discount).HasPrecision(15, 2);
        builder.Property(e => e.TotalPayment).HasPrecision(15, 2);
        builder.Property(e => e.PartialAmount).HasPrecision(15, 2);
        builder.Property(e => e.PaymentOption).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.Notes);
        builder.Property(e => e.CreatedBy);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.IsDeleted);

        builder.HasIndex(e => e.RecordCode).IsUnique();
        builder.HasIndex(e => e.ClientId);
        builder.HasIndex(e => e.BranchId);
        builder.HasIndex(e => e.RecordDate);

        builder.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId);
        builder.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId);
        builder.HasOne(e => e.Creator).WithMany().HasForeignKey(e => e.CreatedBy);
    }
}

public class TaxAccountRecordConfiguration : IEntityTypeConfiguration<TaxAccountRecord>
{
    public void Configure(EntityTypeBuilder<TaxAccountRecord> builder)
    {
        builder.ToTable("tax_account_records");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RecordCode).HasMaxLength(20).IsRequired();
        builder.Property(e => e.RecordDate).IsRequired();
        builder.Property(e => e.ClientId);
        builder.Property(e => e.ClientName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.BranchId);
        builder.Property(e => e.ClientLogo).HasMaxLength(1000);
        builder.Property(e => e.AssignedTo);
        builder.Property(e => e.PaymentStatus).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.Process).HasMaxLength(50);
        builder.Property(e => e.SubTotal).HasPrecision(15, 2);
        builder.Property(e => e.Discount).HasPrecision(15, 2);
        builder.Property(e => e.TotalPayment).HasPrecision(15, 2);
        builder.Property(e => e.PartialAmount).HasPrecision(15, 2);
        builder.Property(e => e.PaymentOption).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.Notes);
        builder.Property(e => e.CreatedBy);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.IsDeleted);

        builder.HasIndex(e => e.RecordCode).IsUnique();
        builder.HasIndex(e => e.ClientId);
        builder.HasIndex(e => e.BranchId);
        builder.HasIndex(e => e.AssignedTo);
        builder.HasIndex(e => e.RecordDate);

        builder.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId);
        builder.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId);
        builder.HasOne(e => e.AssignedUser).WithMany().HasForeignKey(e => e.AssignedTo);
        builder.HasOne(e => e.Creator).WithMany().HasForeignKey(e => e.CreatedBy);
    }
}

public class OtherAuditRecordConfiguration : IEntityTypeConfiguration<OtherAuditRecord>
{
    public void Configure(EntityTypeBuilder<OtherAuditRecord> builder)
    {
        builder.ToTable("other_audit_records");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RecordCode).HasMaxLength(20).IsRequired();
        builder.Property(e => e.RecordDate).IsRequired();
        builder.Property(e => e.ClientId);
        builder.Property(e => e.ClientName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Company).HasMaxLength(255).IsRequired();
        builder.Property(e => e.BranchId);
        builder.Property(e => e.Assignment);
        builder.Property(e => e.PaymentStatus).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.SubTotal).HasPrecision(15, 2);
        builder.Property(e => e.Discount).HasPrecision(15, 2);
        builder.Property(e => e.TotalPayment).HasPrecision(15, 2);
        builder.Property(e => e.PartialAmount).HasPrecision(15, 2);
        builder.Property(e => e.PaymentOption).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.Description);
        builder.Property(e => e.CreatedBy);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.IsDeleted);

        builder.HasIndex(e => e.RecordCode).IsUnique();
        builder.HasIndex(e => e.ClientId);
        builder.HasIndex(e => e.BranchId);
        builder.HasIndex(e => e.RecordDate);

        builder.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId);
        builder.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId);
        builder.HasOne(e => e.Creator).WithMany().HasForeignKey(e => e.CreatedBy);
    }
}
