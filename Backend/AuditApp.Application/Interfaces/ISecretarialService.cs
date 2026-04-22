using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Secretarial;
using AuditApp.Domain.Entities;

namespace AuditApp.Application.Interfaces;

public interface ISecretarialService
{
    // Company Registrations
    Task<PaginatedResult<CompanyRegistrationResponse>> GetCompanyRegistrationsAsync(PaginationParams @params, CancellationToken ct = default);
    Task<CompanyRegistrationResponse?> GetCompanyRegistrationByIdAsync(Guid id, CancellationToken ct = default);
    Task<CompanyRegistrationResponse> CreateCompanyRegistrationAsync(CreateCompanyRegistrationRequest request, CancellationToken ct = default);
    Task<CompanyRegistrationResponse> UpdateCompanyRegistrationAsync(Guid id, UpdateCompanyRegistrationRequest request, CancellationToken ct = default);
    Task DeleteCompanyRegistrationAsync(Guid id, CancellationToken ct = default);

    // EPF/ETF
    Task<PaginatedResult<EpfEtfRecordResponse>> GetEpfEtfRecordsAsync(PaginationParams @params, CancellationToken ct = default);
    Task<EpfEtfRecordResponse?> GetEpfEtfRecordByIdAsync(Guid id, CancellationToken ct = default);
    Task<EpfEtfRecordResponse> CreateEpfEtfRecordAsync(CreateEpfEtfRecordRequest request, CancellationToken ct = default);
    Task<EpfEtfRecordResponse> UpdateEpfEtfRecordAsync(Guid id, UpdateEpfEtfRecordRequest request, CancellationToken ct = default);
    Task DeleteEpfEtfRecordAsync(Guid id, CancellationToken ct = default);

    // Company Officers
    Task<List<CompanyOfficerResponse>> GetOfficersAsync(Guid companyRegistrationId, CancellationToken ct = default);
    Task<CompanyOfficerResponse> AddOfficerAsync(Guid companyRegistrationId, CreateCompanyOfficerRequest request, CancellationToken ct = default);
    Task<CompanyOfficerResponse> UpdateOfficerAsync(Guid companyRegistrationId, Guid officerId, UpdateCompanyOfficerRequest request, CancellationToken ct = default);
    Task DeleteOfficerAsync(Guid companyRegistrationId, Guid officerId, CancellationToken ct = default);

    // EPF/ETF Staff
    Task<List<EpfEtfStaffResponse>> GetStaffAsync(Guid epfEtfRecordId, CancellationToken ct = default);
    Task<EpfEtfStaffResponse> AddStaffAsync(Guid epfEtfRecordId, CreateEpfEtfStaffRequest request, CancellationToken ct = default);
    Task<EpfEtfStaffResponse> UpdateStaffAsync(Guid epfEtfRecordId, Guid staffId, UpdateEpfEtfStaffRequest request, CancellationToken ct = default);
    Task DeleteStaffAsync(Guid epfEtfRecordId, Guid staffId, CancellationToken ct = default);
    Task<EpfEtfStaffResponse> UpdateStaffProcessAsync(Guid epfEtfRecordId, Guid staffId, string process, CancellationToken ct = default);

    // Generic methods for other secretarial types
    Task<PaginatedResult<SecretarialRecordResponse>> GetRecordsAsync<TEntity>(PaginationParams @params, CancellationToken ct = default) where TEntity : SecretarialBaseEntity;
    Task<SecretarialRecordResponse?> GetRecordByIdAsync<TEntity>(Guid id, CancellationToken ct = default) where TEntity : SecretarialBaseEntity;
    Task<SecretarialRecordResponse> CreateRecordAsync<TEntity>(CreateSecretarialRecordRequest request, CancellationToken ct = default) where TEntity : SecretarialBaseEntity;
    Task<SecretarialRecordResponse> UpdateRecordAsync<TEntity>(Guid id, UpdateSecretarialRecordRequest request, CancellationToken ct = default) where TEntity : SecretarialBaseEntity;
    Task DeleteRecordAsync<TEntity>(Guid id, CancellationToken ct = default) where TEntity : SecretarialBaseEntity;
}
