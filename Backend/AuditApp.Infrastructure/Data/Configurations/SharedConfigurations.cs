using AuditApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditApp.Infrastructure.Data.Configurations;

public class NexoraServiceConfiguration : IEntityTypeConfiguration<NexoraService>
{
    public void Configure(EntityTypeBuilder<NexoraService> builder)
    {
        builder.ToTable("nexora_services");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Description);
        builder.Property(e => e.IsActive);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.Ignore(e => e.CreatedBy);

        builder.HasIndex(e => e.Name).IsUnique();
    }
}

public class NexoraServiceRequestConfiguration : IEntityTypeConfiguration<NexoraServiceRequest>
{
    public void Configure(EntityTypeBuilder<NexoraServiceRequest> builder)
    {
        builder.ToTable("nexora_service_requests");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RequestDate).IsRequired();
        builder.Property(e => e.ClientId);
        builder.Property(e => e.ClientName).HasMaxLength(255).IsRequired();
        builder.Property(e => e.CompanyName).HasMaxLength(255);
        builder.Property(e => e.ServiceId);
        builder.Property(e => e.Phone).HasMaxLength(30);
        builder.Property(e => e.Notes);
        builder.Property(e => e.BranchId);
        builder.Property(e => e.CreatedBy);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.IsDeleted);

        builder.HasIndex(e => e.ClientId);
        builder.HasIndex(e => e.ServiceId);
        builder.HasIndex(e => e.BranchId);
        builder.HasIndex(e => e.RequestDate);

        builder.HasOne(e => e.Client).WithMany().HasForeignKey(e => e.ClientId);
        builder.HasOne(e => e.Service).WithMany().HasForeignKey(e => e.ServiceId);
        builder.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId);
    }
}

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RecordType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.PaymentDate);
        builder.Property(e => e.SubTotal).HasPrecision(15, 2);
        builder.Property(e => e.Discount).HasPrecision(15, 2);
        builder.Property(e => e.TotalAmount).HasPrecision(15, 2);
        builder.Property(e => e.PaidAmount).HasPrecision(15, 2);
        builder.Property(e => e.RemainingAmount).HasPrecision(15, 2);
        builder.Property(e => e.PaymentOption).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.PaymentStatus).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.Notes);
        builder.Property(e => e.CreatedBy);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => new { e.RecordType, e.RecordId });
        builder.HasIndex(e => e.PaymentDate);
        builder.HasIndex(e => e.PaymentStatus);

        builder.HasMany(e => e.ChequeDetails).WithOne(c => c.Payment).HasForeignKey(c => c.PaymentId).OnDelete(DeleteBehavior.Cascade);
    }
}

public class ChequeDetailConfiguration : IEntityTypeConfiguration<ChequeDetail>
{
    public void Configure(EntityTypeBuilder<ChequeDetail> builder)
    {
        builder.ToTable("cheque_details");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.PaymentId).IsRequired();
        builder.Property(e => e.BankName).HasMaxLength(100);
        builder.Property(e => e.ChequeNumber).HasMaxLength(50);
        builder.Property(e => e.ChequeDate);
        builder.Property(e => e.Status).HasMaxLength(20).HasConversion<string>();
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.Ignore(e => e.CreatedBy);

        builder.HasIndex(e => e.PaymentId);
    }
}

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("documents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RecordType).HasMaxLength(50).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.DocumentCategory).HasMaxLength(50);
        builder.Property(e => e.FileName).HasMaxLength(500).IsRequired();
        builder.Property(e => e.FileSize).HasMaxLength(20);
        builder.Property(e => e.MimeType).HasMaxLength(100);
        builder.Property(e => e.StorageKey).HasMaxLength(1000).IsRequired();
        builder.Property(e => e.Description);
        builder.Property(e => e.UploadedBy);
        builder.Property(e => e.UploadedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.IsDeleted);

        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.CreatedBy);

        builder.HasIndex(e => new { e.RecordType, e.RecordId });
        builder.HasIndex(e => e.DocumentCategory);

        builder.HasOne(e => e.Uploader).WithMany().HasForeignKey(e => e.UploadedBy);
    }
}

public class ActivityLogConfiguration : IEntityTypeConfiguration<ActivityLog>
{
    public void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        builder.ToTable("activity_logs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityAlwaysColumn();
        builder.Property(e => e.Timestamp).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UserId);
        builder.Property(e => e.BranchId);
        builder.Property(e => e.Action).HasMaxLength(20).IsRequired();
        builder.Property(e => e.Module).HasMaxLength(50).IsRequired();
        builder.Property(e => e.RecordType).HasMaxLength(50);
        builder.Property(e => e.RecordId);
        builder.Property(e => e.Description);
        builder.Property(e => e.IpAddress).HasMaxLength(45);
        builder.Property(e => e.UserAgent);

        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.BranchId);
        builder.HasIndex(e => e.Timestamp);
        builder.HasIndex(e => e.Action);
        builder.HasIndex(e => e.Module);
        builder.HasIndex(e => new { e.RecordType, e.RecordId });

        builder.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId);
        builder.HasOne(e => e.Branch).WithMany().HasForeignKey(e => e.BranchId);
    }
}

public class SyncTrackingConfiguration : IEntityTypeConfiguration<SyncTracking>
{
    public void Configure(EntityTypeBuilder<SyncTracking> builder)
    {
        builder.ToTable("sync_tracking");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).UseIdentityAlwaysColumn();
        builder.Property(e => e.TableName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.RecordId).IsRequired();
        builder.Property(e => e.Operation).HasMaxLength(10).HasConversion<string>().IsRequired();
        builder.Property(e => e.SyncedAt);
        builder.Property(e => e.DeviceId).HasMaxLength(100);
        builder.Property(e => e.ConflictResolved);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(e => e.TableName);
        builder.HasIndex(e => e.RecordId);
        builder.HasIndex(e => e.DeviceId);
        builder.HasIndex(e => e.SyncedAt);
    }
}
