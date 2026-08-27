using Microsoft.EntityFrameworkCore;
using AATS.Domain.Entities;

namespace AATS.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Client> Clients => Set<Client>();
        public DbSet<AuditRecord> AuditRecords => Set<AuditRecord>();
        public DbSet<TaxRecord> TaxRecords => Set<TaxRecord>();
        public DbSet<CompanyOfficer> CompanyOfficers => Set<CompanyOfficer>();
        public DbSet<StaffMember> StaffMembers => Set<StaffMember>();
        public DbSet<SourceDocument> SourceDocuments => Set<SourceDocument>();
        public DbSet<NexoraRequest> NexoraRequests => Set<NexoraRequest>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
        public DbSet<AppNotification> AppNotifications => Set<AppNotification>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Table mappings to match Supabase database schema
            modelBuilder.Entity<Branch>().ToTable("branches");
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Client>().ToTable("clients");
            modelBuilder.Entity<AuditRecord>().ToTable("audit_records");
            modelBuilder.Entity<TaxRecord>().ToTable("tax_records");
            modelBuilder.Entity<CompanyOfficer>().ToTable("company_officers");
            modelBuilder.Entity<StaffMember>().ToTable("staff_members");
            modelBuilder.Entity<SourceDocument>().ToTable("source_documents");
            modelBuilder.Entity<NexoraRequest>().ToTable("nexora_requests");
            modelBuilder.Entity<ActivityLog>().ToTable("activity_logs");
            modelBuilder.Entity<AppNotification>().ToTable("app_notifications");

            // Apply snake_case column names globally
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    var name = property.Name;
                    var snakeName = string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
                    property.SetColumnName(snakeName);
                }
            }
        }
    }
}
