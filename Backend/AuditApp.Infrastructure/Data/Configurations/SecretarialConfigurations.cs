using AuditApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditApp.Infrastructure.Data.Configurations;

public class CompanyRegistrationConfiguration : IEntityTypeConfiguration<CompanyRegistration>
{
    public void Configure(EntityTypeBuilder<CompanyRegistration> builder)
    {
        builder.ToTable("company_registrations");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RegistrationCode).HasMaxLength(20).IsRequired();
        builder.Property(e => e.RegistrationDate).IsRequired();
        builder.Property(e => e.ClientId);
        builder.Property(e => e.ClientName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.CompanyName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.CompanyType).HasMaxLength(100);
        builder.Property(e => e.Objective);
        builder.Property(e => e.Address);
        builder.Property(e => e.Email).HasMaxLength(255);
        builder.Property(e => e.Phone).HasMaxLength(30);
        builder.Property(e => e.PaymentStatus).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.Process).HasMaxLength(50);
        builder.Property(e => e.Description);
        builder.Property(e => e.SubTotal).HasPrecision(15, 2);
        builder.Property(e => e.Discount).HasPrecision(15, 2);
        builder.Property(e => e.TotalPayment).HasPrecision(15, 2);
        builder.Property(e => e.PartialAmount).HasPrecision(15, 2);
        builder.Property(e => e.PaymentOption).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.BranchId);
        builder.Property(e => e.CreatedBy);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.IsDeleted);

        builder.HasIndex(e => e.RegistrationCode).IsUnique();
        builder.HasIndex(e => e.ClientId);
        builder.HasIndex(e => e.BranchId);
        builder.HasIndex(e => e.RegistrationDate);

        builder.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId);
        builder.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId);
        builder.HasMany(e => e.Officers).WithOne(o => o.CompanyRegistration).HasForeignKey(o => o.CompanyRegistrationId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class CompanyOfficerConfiguration : IEntityTypeConfiguration<CompanyOfficer>
{
    public void Configure(EntityTypeBuilder<CompanyOfficer> builder)
    {
        builder.ToTable("company_officers");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.CompanyRegistrationId).IsRequired();
        builder.Property(e => e.Name).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Position).HasMaxLength(100);
        builder.Property(e => e.OfficerType).HasMaxLength(30).IsRequired();
        builder.Property(e => e.SharePercentage).HasPrecision(5, 2);
        builder.Property(e => e.NicNumber).HasMaxLength(30);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => e.CompanyRegistrationId);
    }
}

public class EpfEtfRecordConfiguration : IEntityTypeConfiguration<EpfEtfRecord>
{
    public void Configure(EntityTypeBuilder<EpfEtfRecord> builder)
    {
        builder.ToTable("epf_etf_records");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RecordCode).HasMaxLength(20).IsRequired();
        builder.Property(e => e.RecordDate).IsRequired();
        builder.Property(e => e.ClientId);
        builder.Property(e => e.ClientName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.CompanyName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.NumberOfStaff);
        builder.Property(e => e.Process).HasMaxLength(50);
        builder.Property(e => e.Phone).HasMaxLength(30);
        builder.Property(e => e.SubTotal).HasPrecision(15, 2);
        builder.Property(e => e.Discount).HasPrecision(15, 2);
        builder.Property(e => e.TotalPayment).HasPrecision(15, 2);
        builder.Property(e => e.PartialAmount).HasPrecision(15, 2);
        builder.Property(e => e.PaymentOption).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.PaymentStatus).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.BranchId);
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
        builder.HasMany(e => e.StaffMembers).WithOne("ParentRecord").HasForeignKey(s => s.EpfEtfRecordId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class EpfEtfStaffConfiguration : IEntityTypeConfiguration<EpfEtfStaff>
{
    public void Configure(EntityTypeBuilder<EpfEtfStaff> builder)
    {
        builder.ToTable("epf_etf_staff");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EpfEtfRecordId).IsRequired();
        builder.Property(e => e.StaffCode).HasMaxLength(30).IsRequired();
        builder.Property(e => e.StaffName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Phone).HasMaxLength(30);
        builder.Property(e => e.Process).HasMaxLength(30);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => e.EpfEtfRecordId);
    }
}

// Remaining secretarial modules use a helper to reduce repetition
public class TradeMarkConfiguration : IEntityTypeConfiguration<TradeMark>
{
    public void Configure(EntityTypeBuilder<TradeMark> builder)
    {
        builder.ToTable("trade_marks");
        SecretarialConfigHelper.ConfigureBase(builder);
        builder.Property(e => e.CompanyName).HasMaxLength(255);
        builder.Property(e => e.TrademarkCode).HasMaxLength(50);
        builder.Property(e => e.Status).HasMaxLength(30);
    }
}

public class TradeLicenseConfiguration : IEntityTypeConfiguration<TradeLicense>
{
    public void Configure(EntityTypeBuilder<TradeLicense> builder)
    {
        builder.ToTable("trade_licenses");
        SecretarialConfigHelper.ConfigureBase(builder);
        builder.Property(e => e.CompanyName).HasMaxLength(255);
        builder.Property(e => e.LicenseCode).HasMaxLength(50);
        builder.Property(e => e.Assignment);
        builder.Property(e => e.Status).HasMaxLength(30);
    }
}

public class ImportExportClearanceConfiguration : IEntityTypeConfiguration<ImportExportClearance>
{
    public void Configure(EntityTypeBuilder<ImportExportClearance> builder)
    {
        builder.ToTable("import_export_clearances");
        SecretarialConfigHelper.ConfigureBase(builder);
        builder.Property(e => e.CompanyName).HasMaxLength(255);
        builder.Property(e => e.ClearanceCode).HasMaxLength(50);
        builder.Property(e => e.Assignment);
        builder.Property(e => e.TinNumber).HasMaxLength(50);
        builder.Property(e => e.Status).HasMaxLength(30);
    }
}

public class HrManagementConsultingConfiguration : IEntityTypeConfiguration<HrManagementConsulting>
{
    public void Configure(EntityTypeBuilder<HrManagementConsulting> builder)
    {
        builder.ToTable("hr_management_consulting");
        SecretarialConfigHelper.ConfigureBase(builder);
        builder.Property(e => e.CompanyName).HasMaxLength(255);
        builder.Property(e => e.Assignment);
        builder.Property(e => e.Status).HasMaxLength(30);
    }
}

public class BusinessPlanValuationConfiguration : IEntityTypeConfiguration<BusinessPlanValuation>
{
    public void Configure(EntityTypeBuilder<BusinessPlanValuation> builder)
    {
        builder.ToTable("business_plan_valuations");
        SecretarialConfigHelper.ConfigureBase(builder);
        builder.Property(e => e.CompanyName).HasMaxLength(255);
        builder.Property(e => e.Assignment);
        builder.Property(e => e.Status).HasMaxLength(30);
    }
}

public class BoiRegistrationConfiguration : IEntityTypeConfiguration<BoiRegistration>
{
    public void Configure(EntityTypeBuilder<BoiRegistration> builder)
    {
        builder.ToTable("boi_registrations");
        SecretarialConfigHelper.ConfigureBase(builder);
        builder.Property(e => e.CompanyName).HasMaxLength(255);
        builder.Property(e => e.BoiCode).HasMaxLength(50);
        builder.Property(e => e.Assignment);
        builder.Property(e => e.Country).HasMaxLength(100);
        builder.Property(e => e.CountryAddress);
        builder.Property(e => e.InvestmentValueUsd).HasPrecision(18, 2);
        builder.Property(e => e.Status).HasMaxLength(30);

        builder.HasIndex(e => e.Country);
    }
}

public class OtherSecretarialRecordConfiguration : IEntityTypeConfiguration<OtherSecretarialRecord>
{
    public void Configure(EntityTypeBuilder<OtherSecretarialRecord> builder)
    {
        builder.ToTable("other_secretarial_records");
        SecretarialConfigHelper.ConfigureBase(builder);
        builder.Property(e => e.CompanyName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.Assignment);
        builder.Property(e => e.Description);
    }
}

/// <summary>
/// Helper to configure the shared columns across secretarial service record entities.
/// </summary>
internal static class SecretarialConfigHelper
{
    public static void ConfigureBase<T>(EntityTypeBuilder<T> builder) where T : Domain.Common.BaseEntity, Domain.Common.ISoftDeletable
    {
        builder.HasKey(e => e.Id);
        builder.Property("RecordCode").HasMaxLength(20);
        builder.Property("RecordDate");
        builder.Property("ClientId");
        builder.Property("ClientName").HasMaxLength(255);
        builder.Property("SubTotal").HasPrecision(15, 2);
        builder.Property("Discount").HasPrecision(15, 2);
        builder.Property("TotalPayment").HasPrecision(15, 2);
        builder.Property("PartialAmount").HasPrecision(15, 2);
        builder.Property("PaymentOption").HasMaxLength(20);
        builder.Property("PaymentStatus").HasMaxLength(20);
        builder.Property("BranchId");
        builder.Property("CreatedBy");
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.IsDeleted);

        builder.HasIndex("RecordCode").IsUnique();
        builder.HasIndex("ClientId");
        builder.HasIndex("BranchId");
        builder.HasIndex("RecordDate");
    }
}
