using AuditApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using AuditApp.Application.Interfaces;

namespace AuditApp.Infrastructure.Data;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Client> Clients => Set<Client>();
    
    public DbSet<AuditAssuranceRecord> AuditAssuranceRecords => Set<AuditAssuranceRecord>();
    public DbSet<ForensicAuditRecord> ForensicAuditRecords => Set<ForensicAuditRecord>();
    public DbSet<InternalAuditRecord> InternalAuditRecords => Set<InternalAuditRecord>();
    public DbSet<ManagementAccountRecord> ManagementAccountRecords => Set<ManagementAccountRecord>();
    public DbSet<InternalControlRecord> InternalControlRecords => Set<InternalControlRecord>();
    public DbSet<TaxAccountRecord> TaxAccountRecords => Set<TaxAccountRecord>();
    public DbSet<OtherAuditRecord> OtherAuditRecords => Set<OtherAuditRecord>();
    
    public DbSet<TaxFiling> TaxFilings => Set<TaxFiling>();
    
    public DbSet<CompanyRegistration> CompanyRegistrations => Set<CompanyRegistration>();
    public DbSet<CompanyOfficer> CompanyOfficers => Set<CompanyOfficer>();
    public DbSet<EpfEtfRecord> EpfEtfRecords => Set<EpfEtfRecord>();
    public DbSet<EpfEtfStaff> EpfEtfStaff => Set<EpfEtfStaff>();
    public DbSet<TradeMark> TradeMarks => Set<TradeMark>();
    public DbSet<TradeLicense> TradeLicenses => Set<TradeLicense>();
    public DbSet<ImportExportClearance> ImportExportClearances => Set<ImportExportClearance>();
    public DbSet<HrManagementConsulting> HrManagementConsulting => Set<HrManagementConsulting>();
    public DbSet<BusinessPlanValuation> BusinessPlanValuations => Set<BusinessPlanValuation>();
    public DbSet<BoiRegistration> BoiRegistrations => Set<BoiRegistration>();
    public DbSet<OtherSecretarialRecord> OtherSecretarialRecords => Set<OtherSecretarialRecord>();
    
    public DbSet<NexoraService> NexoraServices => Set<NexoraService>();
    public DbSet<NexoraServiceRequest> NexoraServiceRequests => Set<NexoraServiceRequest>();
    
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<ChequeDetail> ChequeDetails => Set<ChequeDetail>();
    public DbSet<Document> Documents => Set<Document>();
    
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();
    public DbSet<SyncTracking> SyncTrackings => Set<SyncTracking>();

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Apply all configurations from the current assembly (Infrastructure)
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
