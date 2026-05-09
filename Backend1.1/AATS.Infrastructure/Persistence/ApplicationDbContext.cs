using Microsoft.EntityFrameworkCore;
using AATS.Domain.Entities;
using System.Linq;

namespace AATS.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Core
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Client> Clients => Set<Client>();

        // Auditing
        public DbSet<AuditAssuranceRecord> AuditAssuranceRecords => Set<AuditAssuranceRecord>();
        public DbSet<ForensicAuditRecord> ForensicAuditRecords => Set<ForensicAuditRecord>();
        public DbSet<InternalAuditRecord> InternalAuditRecords => Set<InternalAuditRecord>();
        public DbSet<InternalControlRecord> InternalControlRecords => Set<InternalControlRecord>();
        public DbSet<ManagementAccountRecord> ManagementAccountRecords => Set<ManagementAccountRecord>();
        public DbSet<OtherAuditRecord> OtherAuditRecords => Set<OtherAuditRecord>();

        // Taxing
        public DbSet<TaxAccountRecord> TaxAccountRecords => Set<TaxAccountRecord>();
        public DbSet<TaxFiling> TaxFilings => Set<TaxFiling>();

        // Secretarial
        public DbSet<CompanyRegistration> CompanyRegistrations => Set<CompanyRegistration>();
        public DbSet<CompanyOfficer> CompanyOfficers => Set<CompanyOfficer>();
        public DbSet<EpfEtfRecord> EpfEtfRecords => Set<EpfEtfRecord>();
        public DbSet<EpfEtfStaffMember> EpfEtfStaffMembers => Set<EpfEtfStaffMember>();
        public DbSet<TradeMark> TradeMarks => Set<TradeMark>();
        public DbSet<TradeLicense> TradeLicenses => Set<TradeLicense>();
        public DbSet<ImportExportClearance> ImportExportClearances => Set<ImportExportClearance>();
        public DbSet<BoiRegistration> BoiRegistrations => Set<BoiRegistration>();
        public DbSet<BusinessPlanValuation> BusinessPlanValuations => Set<BusinessPlanValuation>();
        public DbSet<HrManagementConsulting> HrManagementConsulting => Set<HrManagementConsulting>();
        public DbSet<OtherSecretarialRecord> OtherSecretarialRecords => Set<OtherSecretarialRecord>();

        // Nexora & System
        public DbSet<NexoraServiceRequest> NexoraServiceRequests => Set<NexoraServiceRequest>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<ChequeDetail> ChequeDetails => Set<ChequeDetail>();
        public DbSet<Document> Documents => Set<Document>();
        public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Manual Table Mappings (to match your modular schema exactly)
            modelBuilder.Entity<Branch>().ToTable("branches");
            modelBuilder.Entity<User>().ToTable("users");
            modelBuilder.Entity<Client>().ToTable("clients");
            modelBuilder.Entity<AuditAssuranceRecord>().ToTable("audit_assurance_records");
            modelBuilder.Entity<ForensicAuditRecord>().ToTable("forensic_audit_records");
            modelBuilder.Entity<InternalAuditRecord>().ToTable("internal_audit_records");
            modelBuilder.Entity<InternalControlRecord>().ToTable("internal_control_records");
            modelBuilder.Entity<ManagementAccountRecord>().ToTable("management_account_records");
            modelBuilder.Entity<OtherAuditRecord>().ToTable("other_audit_records");
            modelBuilder.Entity<TaxAccountRecord>().ToTable("tax_account_records");
            modelBuilder.Entity<TaxFiling>().ToTable("tax_filings");
            modelBuilder.Entity<CompanyRegistration>().ToTable("company_registrations");
            modelBuilder.Entity<CompanyOfficer>().ToTable("company_officers");
            modelBuilder.Entity<EpfEtfRecord>().ToTable("epf_etf_records");
            modelBuilder.Entity<EpfEtfStaffMember>().ToTable("epf_etf_staff");
            modelBuilder.Entity<TradeMark>().ToTable("trade_marks");
            modelBuilder.Entity<TradeLicense>().ToTable("trade_licenses");
            modelBuilder.Entity<ImportExportClearance>().ToTable("import_export_clearances");
            modelBuilder.Entity<BoiRegistration>().ToTable("boi_registrations");
            modelBuilder.Entity<BusinessPlanValuation>().ToTable("business_plan_valuations");
            modelBuilder.Entity<HrManagementConsulting>().ToTable("hr_management_consulting");
            modelBuilder.Entity<OtherSecretarialRecord>().ToTable("other_secretarial_records");
            modelBuilder.Entity<NexoraServiceRequest>().ToTable("nexora_service_requests");
            modelBuilder.Entity<Payment>().ToTable("payments");
            modelBuilder.Entity<ChequeDetail>().ToTable("cheque_details");
            modelBuilder.Entity<Document>().ToTable("documents");
            modelBuilder.Entity<ActivityLog>().ToTable("activity_logs");

            // 2. Globally apply lower_snake_case to all columns
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
