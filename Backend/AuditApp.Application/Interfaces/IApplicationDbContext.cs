using AuditApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditApp.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Branch> Branches { get; }
    DbSet<User> Users { get; }
    DbSet<Client> Clients { get; }
    
    DbSet<AuditAssuranceRecord> AuditAssuranceRecords { get; }
    DbSet<ForensicAuditRecord> ForensicAuditRecords { get; }
    DbSet<InternalAuditRecord> InternalAuditRecords { get; }
    DbSet<ManagementAccountRecord> ManagementAccountRecords { get; }
    DbSet<InternalControlRecord> InternalControlRecords { get; }
    DbSet<TaxAccountRecord> TaxAccountRecords { get; }
    DbSet<OtherAuditRecord> OtherAuditRecords { get; }
    
    DbSet<TaxFiling> TaxFilings { get; }
    
    DbSet<CompanyRegistration> CompanyRegistrations { get; }
    DbSet<CompanyOfficer> CompanyOfficers { get; }
    DbSet<EpfEtfRecord> EpfEtfRecords { get; }
    DbSet<EpfEtfStaff> EpfEtfStaff { get; }
    DbSet<TradeMark> TradeMarks { get; }
    DbSet<TradeLicense> TradeLicenses { get; }
    DbSet<ImportExportClearance> ImportExportClearances { get; }
    DbSet<HrManagementConsulting> HrManagementConsulting { get; }
    DbSet<BusinessPlanValuation> BusinessPlanValuations { get; }
    DbSet<BoiRegistration> BoiRegistrations { get; }
    DbSet<OtherSecretarialRecord> OtherSecretarialRecords { get; }
    
    DbSet<NexoraService> NexoraServices { get; }
    DbSet<NexoraServiceRequest> NexoraServiceRequests { get; }
    
    DbSet<Payment> Payments { get; }
    DbSet<ChequeDetail> ChequeDetails { get; }
    DbSet<Document> Documents { get; }
    
    DbSet<ActivityLog> ActivityLogs { get; }
    DbSet<SyncTracking> SyncTrackings { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
