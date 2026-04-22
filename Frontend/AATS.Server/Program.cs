using AATS.Server.Data;
using AATS.Server.Models;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddSingleton<InMemoryAppStore>();

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var api = app.MapGroup("/api");

api.MapPost("/auth/login", (LoginRequest request, InMemoryAppStore store) =>
{
    var member = store.Authenticate(request.UsernameOrEmail, request.Password);
    return member is null ? Results.Unauthorized() : Results.Ok(member);
});

api.MapGet("/auth/me", (InMemoryAppStore store) => Results.Ok(store.CurrentUser));

api.MapPost("/auth/password-reset", (PasswordResetRequest request, InMemoryAppStore store) =>
{
    store.ActivityLogs.Insert(0, new ActivityLogEntry
    {
        Action = "Update",
        Module = "Auth",
        Branch = request.Branch ?? "Central",
        User = request.Username,
        Details = $"Password reset requested for '{request.Username}'.",
        Timestamp = DateTime.Now
    });

    return Results.Accepted();
});

api.MapPut("/auth/profile/{id}", (string id, ProfileUpdateRequest request, InMemoryAppStore store) =>
{
    var updated = store.UpdateCurrentUser(id, request);
    return updated is null ? Results.NotFound() : Results.Ok(updated);
});

api.MapGet("/nexora-requests", (InMemoryAppStore store) => Results.Ok(store.NexoraRequests.OrderByDescending(x => x.Date).ToList()));

api.MapGet("/team-members", (InMemoryAppStore store) => Results.Ok(store.TeamMembers.OrderBy(x => x.Username).ToList()));
api.MapPost("/team-members", (TeamMember member, InMemoryAppStore store) =>
{
    member.Id ??= $"TM-{store.TeamMembers.Count + 100:D3}";
    member.CreatedAt = member.CreatedAt == default ? DateTime.Now : member.CreatedAt;
    store.TeamMembers.Add(member);
    return Results.Created($"/api/team-members/{member.Id}", member);
});
api.MapPut("/team-members/{id}", (string id, TeamMember member, InMemoryAppStore store) =>
{
    var existing = store.TeamMembers.FirstOrDefault(x => x.Id == id);
    if (existing is null) return Results.NotFound();

    existing.Username = member.Username;
    existing.Email = member.Email;
    existing.Phone = member.Phone;
    existing.Branch = member.Branch;
    existing.Role = member.Role;
    existing.CreatedAt = member.CreatedAt;
    return Results.Ok(existing);
});
api.MapPost("/team-members/bulk-delete", (List<string> ids, InMemoryAppStore store) =>
{
    store.TeamMembers.RemoveAll(x => x.Id is not null && ids.Contains(x.Id));
    return Results.Ok();
});

api.MapGet("/clients", (InMemoryAppStore store) => Results.Ok(store.Clients.OrderBy(x => x.Name).ToList()));
api.MapPost("/clients", (ClientRecord client, InMemoryAppStore store) =>
{
    client.Id ??= $"CLT-{store.Clients.Count + 100:D3}";
    store.Clients.Add(client);
    return Results.Created($"/api/clients/{client.Id}", client);
});
api.MapPut("/clients/{id}", (string id, ClientRecord client, InMemoryAppStore store) =>
{
    var existing = store.Clients.FirstOrDefault(x => x.Id == id);
    if (existing is null) return Results.NotFound();

    existing.Name = client.Name;
    existing.Email = client.Email;
    existing.Phone = client.Phone;
    existing.Branch = client.Branch;
    existing.Category = client.Category;
    existing.TotalRevenue = client.TotalRevenue;
    existing.DueAmount = client.DueAmount;
    existing.Status = client.Status;
    return Results.Ok(existing);
});
api.MapPost("/clients/bulk-delete", (List<string> ids, InMemoryAppStore store) =>
{
    store.Clients.RemoveAll(x => x.Id is not null && ids.Contains(x.Id));
    return Results.Ok();
});

api.MapGet("/activity-logs", (InMemoryAppStore store) =>
    Results.Ok(store.ActivityLogs.OrderByDescending(x => x.Timestamp).ToList()));

api.MapPost("/activity-logs", (ActivityLogEntry entry, InMemoryAppStore store) =>
{
    entry.Id = string.IsNullOrWhiteSpace(entry.Id) ? Guid.NewGuid().ToString("N")[..8].ToUpperInvariant() : entry.Id;
    entry.Timestamp = entry.Timestamp == default ? DateTime.Now : entry.Timestamp;
    store.ActivityLogs.Insert(0, entry);
    return Results.Accepted();
});

api.MapGet("/audit-records", (string category, InMemoryAppStore store) =>
{
    var normalized = store.NormalizeAuditCategory(category);
    return Results.Ok(store.AuditRecords.TryGetValue(normalized, out var records) ? records : new List<AuditRecord>());
});

api.MapPost("/audit-records", (string category, AuditRecord record, InMemoryAppStore store) =>
{
    var normalized = store.NormalizeAuditCategory(category);
    if (!store.AuditRecords.ContainsKey(normalized))
    {
        store.AuditRecords[normalized] = [];
    }

    record.ID ??= Guid.NewGuid().ToString("N");
    store.AuditRecords[normalized].Add(record);
    return Results.Created($"/api/audit-records/{record.ID}?category={Uri.EscapeDataString(normalized)}", record);
});

api.MapPut("/audit-records/{id}", (string id, string category, AuditRecord record, InMemoryAppStore store) =>
{
    var normalized = store.NormalizeAuditCategory(category);
    if (!store.AuditRecords.TryGetValue(normalized, out var records))
    {
        return Results.NotFound();
    }

    var existing = records.FirstOrDefault(x => x.ID == id);
    if (existing is null)
    {
        return Results.NotFound();
    }

    existing.Date = record.Date;
    existing.ClientName = record.ClientName;
    existing.Company = record.Company;
    existing.PaymentStatus = record.PaymentStatus;
    existing.Process = record.Process;
    existing.PaymentOption = record.PaymentOption;
    existing.Assignment = record.Assignment;
    existing.Branch = record.Branch;
    existing.NoOfStaffs = record.NoOfStaffs;
    existing.Country = record.Country;
    existing.Notes = record.Notes;
    existing.Period = record.Period;
    existing.TIN = record.TIN;
    existing.DirectorID = record.DirectorID;
    existing.InvestmentValue = record.InvestmentValue;
    existing.CountryAddress = record.CountryAddress;
    existing.Code = record.Code;
    existing.Address = record.Address;
    existing.Email = record.Email;
    existing.PhoneNo = record.PhoneNo;
    existing.Objective = record.Objective;
    existing.Description = record.Description;
    existing.DirectorsList = record.DirectorsList;
    existing.SecretariesList = record.SecretariesList;
    existing.ShareholdersList = record.ShareholdersList;
    existing.OthersList = record.OthersList;
    existing.RegistrationDocuments = record.RegistrationDocuments;
    existing.SourceDocuments = record.SourceDocuments;
    existing.StaffList = record.StaffList;
    existing.CurrentStep = record.CurrentStep;
    return Results.Ok(existing);
});

api.MapPost("/audit-records/bulk-delete", (string category, List<string> ids, InMemoryAppStore store) =>
{
    var normalized = store.NormalizeAuditCategory(category);
    if (store.AuditRecords.TryGetValue(normalized, out var records))
    {
        records.RemoveAll(x => x.ID is not null && ids.Contains(x.ID));
    }

    return Results.Ok();
});

api.MapGet("/tax-records", (string category, InMemoryAppStore store) =>
{
    var normalized = store.NormalizeTaxCategory(category);
    return Results.Ok(store.TaxRecords.TryGetValue(normalized, out var records) ? records : new List<TaxRecord>());
});

api.MapPost("/tax-records", (string category, TaxRecord record, InMemoryAppStore store) =>
{
    var normalized = store.NormalizeTaxCategory(category);
    if (!store.TaxRecords.ContainsKey(normalized))
    {
        store.TaxRecords[normalized] = [];
    }

    record.ID ??= Guid.NewGuid().ToString("N");
    store.TaxRecords[normalized].Add(record);
    return Results.Created($"/api/tax-records/{record.ID}?category={Uri.EscapeDataString(normalized)}", record);
});

api.MapPut("/tax-records/{id}", (string id, string category, TaxRecord record, InMemoryAppStore store) =>
{
    var normalized = store.NormalizeTaxCategory(category);
    if (!store.TaxRecords.TryGetValue(normalized, out var records))
    {
        return Results.NotFound();
    }

    var existing = records.FirstOrDefault(x => x.ID == id);
    if (existing is null)
    {
        return Results.NotFound();
    }

    existing.ClientName = record.ClientName;
    existing.ClientNameSub = record.ClientNameSub;
    existing.DINNo = record.DINNo;
    existing.TaxPeriod = record.TaxPeriod;
    existing.Status = record.Status;
    existing.Branch = record.Branch;
    existing.Date = record.Date;
    existing.Notes = record.Notes;
    return Results.Ok(existing);
});

api.MapPost("/tax-records/bulk-delete", (string category, List<string> ids, InMemoryAppStore store) =>
{
    var normalized = store.NormalizeTaxCategory(category);
    if (store.TaxRecords.TryGetValue(normalized, out var records))
    {
        records.RemoveAll(x => x.ID is not null && ids.Contains(x.ID));
    }

    return Results.Ok();
});

app.MapDefaultEndpoints();
app.UseFileServer();
app.Run();
