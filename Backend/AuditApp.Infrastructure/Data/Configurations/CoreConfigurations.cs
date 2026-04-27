using AuditApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditApp.Infrastructure.Data.Configurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("branches");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).HasMaxLength(100).IsRequired();
        builder.Property(b => b.Code).HasMaxLength(20).IsRequired();
        builder.Property(b => b.Address);
        builder.Property(b => b.Phone).HasMaxLength(30);
        builder.Property(b => b.IsActive);
        builder.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(e => e.UpdatedAt).HasDefaultValueSql("NOW()");

        builder.HasIndex(b => b.Name).IsUnique();
        builder.HasIndex(b => b.Code).IsUnique();
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Username).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(255).IsRequired();
        builder.Property(u => u.Phone).HasMaxLength(30);
        builder.Property(u => u.UserLogo).HasMaxLength(1000);
        builder.Property(u => u.PasswordHash).HasMaxLength(255).IsRequired();
        builder.Property(u => u.Role).HasMaxLength(20).HasConversion<string>();
        builder.Property(u => u.BranchId);
        builder.Property(u => u.IsActive);
        builder.Property(u => u.LastLoginAt);
        builder.Property(u => u.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(u => u.UpdatedAt).HasDefaultValueSql("NOW()");
        builder.Property(u => u.IsDeleted);

        builder.HasIndex(u => u.Username).IsUnique();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.HasIndex(u => u.BranchId);
        builder.HasIndex(u => u.Role);

        builder.HasOne(u => u.Branch).WithMany(b => b.Users).HasForeignKey(u => u.BranchId);
    }
}

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.ClientCode).HasMaxLength(20).IsRequired();
        builder.Property(c => c.ClientName).HasMaxLength(255).IsRequired();
        builder.Property(c => c.Email).HasMaxLength(255);
        builder.Property(c => c.Phone).HasMaxLength(30);
        builder.Property(c => c.Status);
        builder.Property(c => c.BranchId);
        builder.Property(c => c.TotalRevenue).HasPrecision(15, 2);
        builder.Property(c => c.OutstandingBalance).HasPrecision(15, 2);
        builder.Property(c => c.LogoStorageKey).HasMaxLength(1000);
        builder.Property(c => c.LastActiveAt);
        builder.Property(c => c.CreatedAt).HasDefaultValueSql("NOW()");
        builder.Property(c => c.UpdatedAt).HasDefaultValueSql("NOW()");
        builder.Property(c => c.IsDeleted);

        builder.HasIndex(c => c.ClientCode).IsUnique();
        builder.HasIndex(c => c.BranchId);
        builder.HasIndex(c => c.Status);

        builder.HasOne(c => c.Branch).WithMany(b => b.Clients).HasForeignKey(c => c.BranchId);
    }
}
