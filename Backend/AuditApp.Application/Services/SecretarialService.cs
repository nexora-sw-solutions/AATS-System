using AuditApp.Application.Common;
using AuditApp.Application.DTOs.Secretarial;
using AuditApp.Application.Interfaces;
using AuditApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuditApp.Application.Services;

public class SecretarialService : ISecretarialService
{
    private readonly IApplicationDbContext _db;

    public SecretarialService(IApplicationDbContext db)
    {
        _db = db;
    }

    #region Company Registrations

    public async Task<PaginatedResult<CompanyRegistrationResponse>> GetCompanyRegistrationsAsync(PaginationParams @params, CancellationToken ct = default)
    {
        var query = _db.CompanyRegistrations
            .Include(r => r.Officers)
            .Where(r => !r.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(@params.Search))
            query = query.Where(r => r.CompanyName.Contains(@params.Search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((@params.Page - 1) * @params.Limit)
            .Take(@params.Limit)
            .ToListAsync(ct);

        return new PaginatedResult<CompanyRegistrationResponse>(
            items.Select(r => MapToCompanyResponse(r)).ToList(),
            @params.Page, @params.Limit, total);
    }

    public async Task<CompanyRegistrationResponse?> GetCompanyRegistrationByIdAsync(Guid id, CancellationToken ct = default)
    {
        var record = await _db.CompanyRegistrations
            .Include(r => r.Officers)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);

        return record != null ? MapToCompanyResponse(record) : null;
    }

    public async Task<CompanyRegistrationResponse> CreateCompanyRegistrationAsync(CreateCompanyRegistrationRequest request, CancellationToken ct = default)
    {
        var record = new CompanyRegistration
        {
            RegistrationCode = $"REG-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            RegistrationDate = request.Date ?? DateOnly.FromDateTime(DateTime.UtcNow),
            ClientId = request.ClientId,
            ClientName = request.ClientName ?? string.Empty,
            CompanyName = request.CompanyName,
            CompanyType = request.CompanyType,
            Email = request.Email,
            Phone = request.Phone,
            Description = request.Description,
            Objective = request.Objective,
            Address = request.Address,
            PaymentStatus = request.Status,
            SubTotal = request.SubTotal,
            Discount = request.Discount,
            TotalPayment = request.TotalPayment,
            PartialAmount = request.PartialAmount,
            PaymentOption = request.PaymentOption,
            BranchId = request.BranchId,
            Officers = request.Officers.Select(o => new CompanyOfficer
            {
                Name = o.Name,
                Position = o.Position,
                OfficerType = o.OfficerType ?? "other",
                SharePercentage = o.SharePercentage,
                NicNumber = o.NicNumber
            }).ToList()
        };

        _db.CompanyRegistrations.Add(record);
        await _db.SaveChangesAsync(ct);

        return MapToCompanyResponse(record);
    }

    public async Task<CompanyRegistrationResponse> UpdateCompanyRegistrationAsync(Guid id, UpdateCompanyRegistrationRequest request, CancellationToken ct = default)
    {
        var record = await _db.CompanyRegistrations
            .Include(r => r.Officers)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException("Company registration not found.");

        if (request.CompanyName != null) record.CompanyName = request.CompanyName;
        if (request.CompanyType != null) record.CompanyType = request.CompanyType;
        if (request.ClientName != null) record.ClientName = request.ClientName;
        if (request.ClientId != null) record.ClientId = request.ClientId;
        if (request.Date != null) record.RegistrationDate = request.Date.Value;
        if (request.Email != null) record.Email = request.Email;
        if (request.Phone != null) record.Phone = request.Phone;
        if (request.Description != null) record.Description = request.Description;
        if (request.Objective != null) record.Objective = request.Objective;
        if (request.Address != null) record.Address = request.Address;
        if (request.Status != null) record.PaymentStatus = request.Status;
        if (request.Process != null) record.Process = request.Process;
        if (request.SubTotal != null) record.SubTotal = request.SubTotal.Value;
        if (request.Discount != null) record.Discount = request.Discount.Value;
        if (request.TotalPayment != null) record.TotalPayment = request.TotalPayment.Value;
        if (request.PartialAmount != null) record.PartialAmount = request.PartialAmount.Value;
        if (request.PaymentOption != null) record.PaymentOption = request.PaymentOption;
        if (request.BranchId != null) record.BranchId = request.BranchId;

        await _db.SaveChangesAsync(ct);
        return MapToCompanyResponse(record);
    }

    public async Task DeleteCompanyRegistrationAsync(Guid id, CancellationToken ct = default)
    {
        var record = await _db.CompanyRegistrations.FindAsync([id], ct);
        if (record != null)
        {
            record.IsDeleted = true;
            await _db.SaveChangesAsync(ct);
        }
    }

    private static CompanyRegistrationResponse MapToCompanyResponse(CompanyRegistration r) => new()
    {
        Id = r.Id,
        RecordCode = r.RegistrationCode,
        RegistrationDate = r.RegistrationDate,
        ClientId = r.ClientId,
        ClientName = r.ClientName,
        CompanyName = r.CompanyName,
        CompanyType = r.CompanyType,
        Email = r.Email,
        Phone = r.Phone,
        Description = r.Description,
        Objective = r.Objective,
        Address = r.Address,
        Status = r.PaymentStatus ?? "Pending",
        SubTotal = r.SubTotal,
        Discount = r.Discount,
        TotalPayment = r.TotalPayment,
        PartialAmount = r.PartialAmount,
        PaymentOption = r.PaymentOption,
        BranchId = r.BranchId,
        Officers = r.Officers.Select(o => new CompanyOfficerResponse
        {
            Id = o.Id,
            Name = o.Name,
            Position = o.Position,
            OfficerType = o.OfficerType,
            SharePercentage = o.SharePercentage,
            NicNumber = o.NicNumber
        }).ToList(),
        CreatedAt = r.CreatedAt
    };

    #endregion

    #region Company Officers

    public async Task<List<CompanyOfficerResponse>> GetOfficersAsync(Guid companyRegistrationId, CancellationToken ct = default)
    {
        var officers = await _db.CompanyOfficers
            .Where(o => o.CompanyRegistrationId == companyRegistrationId)
            .AsNoTracking()
            .ToListAsync(ct);

        return officers.Select(o => new CompanyOfficerResponse
        {
            Id = o.Id, Name = o.Name, Position = o.Position,
            OfficerType = o.OfficerType, SharePercentage = o.SharePercentage,
            NicNumber = o.NicNumber
        }).ToList();
    }

    public async Task<CompanyOfficerResponse> AddOfficerAsync(Guid companyRegistrationId, CreateCompanyOfficerRequest request, CancellationToken ct = default)
    {
        var officer = new CompanyOfficer
        {
            CompanyRegistrationId = companyRegistrationId,
            Name = request.Name,
            Position = request.Position,
            OfficerType = request.OfficerType ?? "other",
            SharePercentage = request.SharePercentage,
            NicNumber = request.NicNumber
        };
        _db.CompanyOfficers.Add(officer);
        await _db.SaveChangesAsync(ct);
        return new CompanyOfficerResponse 
        { 
            Id = officer.Id, 
            Name = officer.Name, 
            Position = officer.Position, 
            OfficerType = officer.OfficerType, 
            SharePercentage = officer.SharePercentage,
            NicNumber = officer.NicNumber
        };
    }

    public async Task<CompanyOfficerResponse> UpdateOfficerAsync(Guid companyRegistrationId, Guid officerId, UpdateCompanyOfficerRequest request, CancellationToken ct = default)
    {
        var officer = await _db.CompanyOfficers.FirstOrDefaultAsync(o => o.Id == officerId && o.CompanyRegistrationId == companyRegistrationId, ct)
            ?? throw new KeyNotFoundException("Officer not found.");

        if (request.Name != null) officer.Name = request.Name;
        if (request.Position != null) officer.Position = request.Position;
        if (request.OfficerType != null) officer.OfficerType = request.OfficerType;
        if (request.SharePercentage.HasValue) officer.SharePercentage = request.SharePercentage;
        if (request.NicNumber != null) officer.NicNumber = request.NicNumber;

        await _db.SaveChangesAsync(ct);
        return new CompanyOfficerResponse 
        { 
            Id = officer.Id, 
            Name = officer.Name, 
            Position = officer.Position, 
            OfficerType = officer.OfficerType, 
            SharePercentage = officer.SharePercentage,
            NicNumber = officer.NicNumber
        };
    }

    public async Task DeleteOfficerAsync(Guid companyRegistrationId, Guid officerId, CancellationToken ct = default)
    {
        var officer = await _db.CompanyOfficers.FirstOrDefaultAsync(o => o.Id == officerId && o.CompanyRegistrationId == companyRegistrationId, ct);
        if (officer != null)
        {
            _db.CompanyOfficers.Remove(officer);
            await _db.SaveChangesAsync(ct);
        }
    }

    #endregion

    #region EPF/ETF

    public async Task<PaginatedResult<EpfEtfRecordResponse>> GetEpfEtfRecordsAsync(PaginationParams @params, CancellationToken ct = default)
    {
        var query = _db.EpfEtfRecords
            .Include(r => r.StaffMembers)
            .Where(r => !r.IsDeleted)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(@params.Search))
            query = query.Where(r => r.ClientName.Contains(@params.Search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((@params.Page - 1) * @params.Limit)
            .Take(@params.Limit)
            .ToListAsync(ct);

        return new PaginatedResult<EpfEtfRecordResponse>(
            items.Select(r => MapToEpfResponse(r)).ToList(),
            @params.Page, @params.Limit, total);
    }

    public async Task<EpfEtfRecordResponse?> GetEpfEtfRecordByIdAsync(Guid id, CancellationToken ct = default)
    {
        var record = await _db.EpfEtfRecords
            .Include(r => r.StaffMembers)
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);

        return record != null ? MapToEpfResponse(record) : null;
    }

    public async Task<EpfEtfRecordResponse> CreateEpfEtfRecordAsync(CreateEpfEtfRecordRequest request, CancellationToken ct = default)
    {
        var record = new EpfEtfRecord
        {
            RecordCode = $"EPF-{Guid.NewGuid().ToString("N")[..8].ToUpper()}",
            ClientName = request.ClientName,
            PaymentStatus = request.Status,
            StaffMembers = request.Staff.Select(s => new EpfEtfStaff
            {
                StaffName = s.Name,
                StaffCode = s.Nic,
                Phone = s.EpfNumber
            }).ToList()
        };

        _db.EpfEtfRecords.Add(record);
        await _db.SaveChangesAsync(ct);

        return MapToEpfResponse(record);
    }

    public async Task<EpfEtfRecordResponse> UpdateEpfEtfRecordAsync(Guid id, UpdateEpfEtfRecordRequest request, CancellationToken ct = default)
    {
        var record = await _db.EpfEtfRecords
            .Include(r => r.StaffMembers)
            .FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException("EPF/ETF record not found.");

        if (request.ClientName != null) record.ClientName = request.ClientName;
        if (request.Status != null) record.PaymentStatus = request.Status;
        if (request.CompanyName != null) record.CompanyName = request.CompanyName;
        if (request.NumberOfStaff.HasValue) record.NumberOfStaff = request.NumberOfStaff.Value;
        if (request.Phone != null) record.Phone = request.Phone;

        await _db.SaveChangesAsync(ct);
        return MapToEpfResponse(record);
    }

    public async Task DeleteEpfEtfRecordAsync(Guid id, CancellationToken ct = default)
    {
        var record = await _db.EpfEtfRecords.FindAsync([id], ct);
        if (record != null)
        {
            record.IsDeleted = true;
            await _db.SaveChangesAsync(ct);
        }
    }

    private static EpfEtfRecordResponse MapToEpfResponse(EpfEtfRecord r) => new()
    {
        Id = r.Id,
        RecordCode = r.RecordCode,
        ClientName = r.ClientName,
        Status = r.PaymentStatus ?? "Pending",
        StaffCount = r.StaffMembers.Count,
        Staff = r.StaffMembers.Select(s => new EpfEtfStaffResponse
        {
            Id = s.Id,
            Name = s.StaffName,
            Nic = s.StaffCode,
            EpfNumber = s.Phone
        }).ToList(),
        CreatedAt = r.CreatedAt
    };

    #endregion

    #region EPF/ETF Staff

    public async Task<List<EpfEtfStaffResponse>> GetStaffAsync(Guid epfEtfRecordId, CancellationToken ct = default)
    {
        var staff = await _db.EpfEtfStaff
            .Where(s => s.EpfEtfRecordId == epfEtfRecordId)
            .AsNoTracking()
            .ToListAsync(ct);

        return staff.Select(s => new EpfEtfStaffResponse { Id = s.Id, Name = s.StaffName, Nic = s.StaffCode, EpfNumber = s.Phone }).ToList();
    }

    public async Task<EpfEtfStaffResponse> AddStaffAsync(Guid epfEtfRecordId, CreateEpfEtfStaffRequest request, CancellationToken ct = default)
    {
        var staff = new EpfEtfStaff
        {
            EpfEtfRecordId = epfEtfRecordId,
            StaffName = request.Name,
            StaffCode = request.Nic,
            Phone = request.EpfNumber
        };
        _db.EpfEtfStaff.Add(staff);
        await _db.SaveChangesAsync(ct);
        return new EpfEtfStaffResponse { Id = staff.Id, Name = staff.StaffName, Nic = staff.StaffCode, EpfNumber = staff.Phone };
    }

    public async Task<EpfEtfStaffResponse> UpdateStaffAsync(Guid epfEtfRecordId, Guid staffId, UpdateEpfEtfStaffRequest request, CancellationToken ct = default)
    {
        var staff = await _db.EpfEtfStaff.FirstOrDefaultAsync(s => s.Id == staffId && s.EpfEtfRecordId == epfEtfRecordId, ct)
            ?? throw new KeyNotFoundException("Staff member not found.");

        if (request.Name != null) staff.StaffName = request.Name;
        if (request.Nic != null) staff.StaffCode = request.Nic;
        if (request.EpfNumber != null) staff.Phone = request.EpfNumber;

        await _db.SaveChangesAsync(ct);
        return new EpfEtfStaffResponse { Id = staff.Id, Name = staff.StaffName, Nic = staff.StaffCode, EpfNumber = staff.Phone };
    }

    public async Task DeleteStaffAsync(Guid epfEtfRecordId, Guid staffId, CancellationToken ct = default)
    {
        var staff = await _db.EpfEtfStaff.FirstOrDefaultAsync(s => s.Id == staffId && s.EpfEtfRecordId == epfEtfRecordId, ct);
        if (staff != null)
        {
            _db.EpfEtfStaff.Remove(staff);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task<EpfEtfStaffResponse> UpdateStaffProcessAsync(Guid epfEtfRecordId, Guid staffId, string process, CancellationToken ct = default)
    {
        var staff = await _db.EpfEtfStaff.FirstOrDefaultAsync(s => s.Id == staffId && s.EpfEtfRecordId == epfEtfRecordId, ct)
            ?? throw new KeyNotFoundException("Staff member not found.");

        staff.Process = process;
        await _db.SaveChangesAsync(ct);
        return new EpfEtfStaffResponse { Id = staff.Id, Name = staff.StaffName, Nic = staff.StaffCode, EpfNumber = staff.Phone };
    }

    #endregion

    #region Generic Secretarial Records

    public async Task<PaginatedResult<SecretarialRecordResponse>> GetRecordsAsync<TEntity>(PaginationParams @params, CancellationToken ct = default) where TEntity : SecretarialBaseEntity
    {
        var set = GetDbSet<TEntity>();
        var query = set.Where(r => !r.IsDeleted).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(@params.Search))
            query = query.Where(r => r.ClientName.Contains(@params.Search));

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((@params.Page - 1) * @params.Limit)
            .Take(@params.Limit)
            .ToListAsync(ct);

        return new PaginatedResult<SecretarialRecordResponse>(
            items.Select(r => MapToGenericResponse(r)).ToList(), @params.Page, @params.Limit, total);
    }

    public async Task<SecretarialRecordResponse?> GetRecordByIdAsync<TEntity>(Guid id, CancellationToken ct = default) where TEntity : SecretarialBaseEntity
    {
        var set = GetDbSet<TEntity>();
        var record = await set.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);
        return record != null ? MapToGenericResponse(record) : null;
    }

    public async Task<SecretarialRecordResponse> CreateRecordAsync<TEntity>(CreateSecretarialRecordRequest request, CancellationToken ct = default) where TEntity : SecretarialBaseEntity
    {
        var set = GetDbSet<TEntity>();
        var record = Activator.CreateInstance<TEntity>();

        // Generate RecordCode
        var prefix = typeof(TEntity).Name.Replace("Record", "").ToUpper()[..3];
        record.RecordCode = $"{prefix}-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";

        record.ClientName = request.ClientName;
        record.PaymentStatus = request.Status;
        
        // Map all matching properties from request to record via reflection
        var requestProps = request.GetType().GetProperties();
        var entityProps = typeof(TEntity).GetProperties();

        foreach (var reqProp in requestProps)
        {
            if (reqProp.Name == "ClientName" || reqProp.Name == "Status") continue;
            
            var entProp = entityProps.FirstOrDefault(p => p.Name == reqProp.Name && p.PropertyType == reqProp.PropertyType);
            if (entProp != null && entProp.CanWrite)
            {
                var val = reqProp.GetValue(request);
                if (val != null) entProp.SetValue(record, val);
            }
        }

        if (record is OtherSecretarialRecord osr && request.Description != null)
            osr.Description = request.Description;

        set.Add(record);
        await _db.SaveChangesAsync(ct);

        return MapToGenericResponse(record);
    }

    public async Task<SecretarialRecordResponse> UpdateRecordAsync<TEntity>(Guid id, UpdateSecretarialRecordRequest request, CancellationToken ct = default) where TEntity : SecretarialBaseEntity
    {
        var set = GetDbSet<TEntity>();
        var record = await set.FirstOrDefaultAsync(r => r.Id == id, ct)
            ?? throw new KeyNotFoundException($"{typeof(TEntity).Name} not found.");

        if (request.ClientName != null) record.ClientName = request.ClientName;
        if (request.Status != null) record.PaymentStatus = request.Status;

        // Map all matching properties from request to record via reflection
        var requestProps = request.GetType().GetProperties();
        var entityProps = typeof(TEntity).GetProperties();

        foreach (var reqProp in requestProps)
        {
            if (reqProp.Name == "ClientName" || reqProp.Name == "Status") continue;
            
            var entProp = entityProps.FirstOrDefault(p => p.Name == reqProp.Name && p.PropertyType == reqProp.PropertyType);
            if (entProp != null && entProp.CanWrite)
            {
                var val = reqProp.GetValue(request);
                if (val != null) entProp.SetValue(record, val);
            }
        }

        if (record is OtherSecretarialRecord osr && request.Description != null)
            osr.Description = request.Description;

        await _db.SaveChangesAsync(ct);
        return MapToGenericResponse(record);
    }

    public async Task DeleteRecordAsync<TEntity>(Guid id, CancellationToken ct = default) where TEntity : SecretarialBaseEntity
    {
        var set = GetDbSet<TEntity>();
        var record = await set.FindAsync([id], ct);
        if (record != null)
        {
            record.IsDeleted = true;
            await _db.SaveChangesAsync(ct);
        }
    }

    private static SecretarialRecordResponse MapToGenericResponse<TEntity>(TEntity r) where TEntity : SecretarialBaseEntity => new()
    {
        Id = r.Id,
        RecordCode = r.RecordCode,
        ClientName = r.ClientName,
        Description = (r as OtherSecretarialRecord)?.Description,
        Status = r.PaymentStatus ?? "Pending",
        CreatedAt = r.CreatedAt
    };

    private DbSet<TEntity> GetDbSet<TEntity>() where TEntity : SecretarialBaseEntity
    {
        return _db.GetType().GetProperties()
            .FirstOrDefault(p => p.PropertyType == typeof(DbSet<TEntity>))
            ?.GetValue(_db) as DbSet<TEntity>
            ?? throw new InvalidOperationException($"DbSet for {typeof(TEntity).Name} not found.");
    }

    #endregion
}
