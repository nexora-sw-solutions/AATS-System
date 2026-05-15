using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
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
        public DbSet<VatFiling> VatFilings => Set<VatFiling>();
        public DbSet<CitFiling> CitFilings => Set<CitFiling>();
        public DbSet<IitFiling> IitFilings => Set<IitFiling>();
        public DbSet<SsclFiling> SsclFilings => Set<SsclFiling>();
        public DbSet<WhtFiling> WhtFilings => Set<WhtFiling>();

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
            modelBuilder.Entity<TaxFiling>().ToTable("tax_filings").UseTpcMappingStrategy();
            modelBuilder.Entity<VatFiling>().ToTable("vat_filings");
            modelBuilder.Entity<CitFiling>().ToTable("cit_filings");
            modelBuilder.Entity<IitFiling>().ToTable("iit_filings");
            modelBuilder.Entity<SsclFiling>().ToTable("sscl_filings");
            modelBuilder.Entity<WhtFiling>().ToTable("wht_filings");
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

            // 2. Globally apply lower_snake_case to all columns (unless overridden)
            foreach (var entity in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entity.GetProperties())
                {
                    // Check if the column name has been explicitly set via ColumnAttribute or Fluent API
                    var explicitName = property.GetColumnName(StoreObjectIdentifier.Table(entity.GetTableName() ?? string.Empty, entity.GetSchema()));
                    
                    // If the column name hasn't been explicitly set (it still matches property name), apply snake_case
                    if (explicitName == property.Name)
                    {
                        var name = property.Name;
                        var snakeName = string.Concat(name.Select((x, i) => i > 0 && char.IsUpper(x) ? "_" + x.ToString() : x.ToString())).ToLower();
                        property.SetColumnName(snakeName);
                    }
                }
            }
        }
    }
}
