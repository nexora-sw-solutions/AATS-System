using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using AATS.Desktop.Data;
using AATS.Desktop.Models;

namespace AATS.Desktop.Services
{
    public class DataService : IDataService
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive = true
        };

        private static DataService? _instance;
        public static DataService Instance => _instance ??= new DataService();

        private readonly HttpClient _httpClient;
        private readonly HttpClient _clientsApiHttpClient;
        private readonly DesktopAppSettings _settings;

        public TeamMember CurrentUser { get; private set; } = MockData.CurrentUser;

        private DataService()
        {
            _settings = DesktopAppSettings.Load();
            _httpClient = new HttpClient
            {
                BaseAddress = CreateBaseUri(_settings.Backend.BaseUrl),
                Timeout = TimeSpan.FromSeconds(Math.Max(5, _settings.Backend.TimeoutSeconds))
            };

            _clientsApiHttpClient = new HttpClient
            {
                BaseAddress = CreateBaseUri(_settings.ClientsApi.BaseUrl),
                Timeout = TimeSpan.FromSeconds(Math.Max(5, _settings.Backend.TimeoutSeconds))
            };

            if (!string.IsNullOrWhiteSpace(_settings.ClientsApi.AccessToken))
            {
                _clientsApiHttpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _settings.ClientsApi.AccessToken);
            }
        }

        public async Task<bool> LoginAsync(string usernameOrEmail, string password)
        {
            if (await TryLoginClientsApiAsync(usernameOrEmail, password))
            {
                return true;
            }

            var result = await TryApiAsync(async () =>
            {
                using var response = await _httpClient.PostAsJsonAsync("api/auth/login", new { usernameOrEmail, password });
                response.EnsureSuccessStatusCode();
                var member = await response.Content.ReadFromJsonAsync<TeamMember>(JsonOptions);
                if (member == null)
                {
                    return false;
                }

                CurrentUser = member;
                return true;
            });

            if (result.HasValue)
            {
                return result.Value;
            }

            var fallbackUser = MockData.TeamMembers.FirstOrDefault(m =>
                string.Equals(m.Email, usernameOrEmail, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(m.Username, usernameOrEmail, StringComparison.OrdinalIgnoreCase));

            if (fallbackUser == null || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            CurrentUser = fallbackUser;
            return true;
        }

        public async Task RequestPasswordResetAsync(PasswordResetRequest request)
        {
            var posted = await PostAsync("api/auth/password-reset", request);
            if (!posted)
            {
                await AddActivityLogAsync(new ActivityLogEntry
                {
                    Action = "Update",
                    Module = "Auth",
                    Branch = request.Branch ?? "Central",
                    User = request.Username,
                    Details = $"Password reset requested for '{request.Username}'."
                });
            }
        }

        public async Task UpdateCurrentUserProfileAsync(TeamMember member, string? currentPassword, string? newPassword)
        {
            if (member?.Id == null)
            {
                return;
            }

            var updated = await TryApiAsync(async () =>
            {
                using var response = await _httpClient.PutAsJsonAsync(
                    $"api/auth/profile/{Uri.EscapeDataString(member.Id)}",
                    new
                    {
                        member.Id,
                        member.Username,
                        member.Email,
                        member.Phone,
                        member.Branch,
                        member.Role,
                        member.CreatedAt,
                        currentPassword,
                        newPassword
                    });
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<TeamMember>(JsonOptions);
            });

            CurrentUser = updated ?? member;
        }

        public Task<List<NexoraRequest>> GetNexoraRequestsAsync() =>
            GetListAsync("api/nexora-requests", () => MockData.NexoraRequests);

        public async Task<List<TeamMember>> GetTeamMembersAsync()
        {
            try
            {
                using var response = await _clientsApiHttpClient.GetAsync("api/v1/users");
                if (!response.IsSuccessStatusCode)
                {
                    LogService.Instance.AddLog("API Error", "Team", "System", $"Fetch users failed: {response.StatusCode}");
                    response.EnsureSuccessStatusCode();
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var document = await JsonDocument.ParseAsync(stream);

                return ExtractCollection(document.RootElement)
                    .Select(MapTeamMember)
                    .Where(m => !string.IsNullOrWhiteSpace(m.Id) || !string.IsNullOrWhiteSpace(m.Username))
                    .ToList();
            }
            catch (Exception ex)
            {
                LogService.Instance.AddLog("Warning", "Team", "System", $"API failure, falling back: {ex.Message}");
                return MockData.TeamMembers;
            }
        }

        public async Task AddTeamMemberAsync(TeamMember member)
        {
            try
            {
                using var response = await _clientsApiHttpClient.PostAsJsonAsync("api/v1/users", CreateUserPayload(member));
                response.EnsureSuccessStatusCode();
                return;
            }
            catch (Exception ex)
            {
                LogService.Instance.AddLog("Warning", "Team", "System", $"API add failed, falling back: {ex.Message}");
                MockData.TeamMembers.Add(member);
            }
        }

        public async Task UpdateTeamMemberAsync(TeamMember member)
        {
            if (string.IsNullOrWhiteSpace(member?.Id)) return;

            try
            {
                using var response = await _clientsApiHttpClient.PutAsJsonAsync($"api/v1/users/{Uri.EscapeDataString(member.Id)}", CreateUserPayload(member));
                response.EnsureSuccessStatusCode();
                return;
            }
            catch (Exception ex)
            {
                LogService.Instance.AddLog("Warning", "Team", "System", $"API update failed, falling back: {ex.Message}");
                var existing = MockData.TeamMembers.FirstOrDefault(m => m.Id == member.Id);
                if (existing != null && !ReferenceEquals(existing, member))
                {
                    CopyTeamMember(existing, member);
                }
            }
        }

        public async Task DeleteTeamMembersAsync(IEnumerable<TeamMember> members)
        {
            try
            {
                foreach (var member in members.Where(m => !string.IsNullOrWhiteSpace(m.Id)))
                {
                    using var response = await _clientsApiHttpClient.DeleteAsync($"api/v1/users/{Uri.EscapeDataString(member.Id!)}");
                    if (!response.IsSuccessStatusCode)
                    {
                        LogService.Instance.AddLog("API Error", "Team", "System", $"Delete user {member.Id} failed: {response.StatusCode}");
                        response.EnsureSuccessStatusCode();
                    }
                }
                return;
            }
            catch (Exception ex)
            {
                LogService.Instance.AddLog("Warning", "Team", "System", $"Delete failed, falling back: {ex.Message}");
                foreach (var member in members.ToList())
                {
                    MockData.TeamMembers.Remove(member);
                }
            }
        }

        public async Task UpdateUserStatusAsync(string userId, string status)
        {
            try
            {
                using var response = await _clientsApiHttpClient.PatchAsync($"api/v1/users/{Uri.EscapeDataString(userId)}/status", 
                    JsonContent.Create(new { status }));
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                LogService.Instance.AddLog("API Error", "Team", "System", $"Status update failed: {ex.Message}");
            }
        }

        public async Task<string?> UploadUserLogoAsync(string userId, string filePath)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(System.IO.File.OpenRead(filePath));
                fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg"); // Default to jpeg, should ideally detect
                content.Add(fileContent, "logo", System.IO.Path.GetFileName(filePath));

                using var response = await _clientsApiHttpClient.PostAsync($"api/v1/users/{Uri.EscapeDataString(userId)}/logo", content);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                if (TryGetProperty(document.RootElement, out var data, "data"))
                {
                    return GetString(data, "logoUrl");
                }
                return null;
            }
            catch (Exception ex)
            {
                LogService.Instance.AddLog("API Error", "Team", "System", $"Logo upload failed: {ex.Message}");
                return null;
            }
        }

        public async Task DeleteUserLogoAsync(string userId)
        {
            try
            {
                using var response = await _clientsApiHttpClient.DeleteAsync($"api/v1/users/{Uri.EscapeDataString(userId)}/logo");
                response.EnsureSuccessStatusCode();
            }
            catch (Exception ex)
            {
                LogService.Instance.AddLog("API Error", "Team", "System", $"Logo deletion failed: {ex.Message}");
            }
        }

        public async Task<List<ClientRecord>> GetClientsAsync()
        {
            try
            {
                return await LoadClientsFromClientsApiAsync();
            }
            catch (Exception ex)
            {
                LogService.Instance.AddLog("Warning", "Clients", "System", $"API failure, falling back to mock data: {ex.Message}");
                return MockData.Clients;
            }
        }

        public async Task AddClientAsync(ClientRecord client)
        {
            try
            {
                using var response = await _clientsApiHttpClient.PostAsJsonAsync("api/v1/clients", CreateClientPayload(client));
                response.EnsureSuccessStatusCode();
                return;
            }
            catch
            {
                MockData.Clients.Add(client);
            }
        }

        public async Task UpdateClientAsync(ClientRecord client)
        {
            if (client == null)
            {
                return;
            }

            var backendId = client.BackendId ?? client.Id;
            if (string.IsNullOrWhiteSpace(backendId))
            {
                return;
            }

            try
            {
                using var response = await _clientsApiHttpClient.PutAsJsonAsync(
                    $"api/v1/clients/{Uri.EscapeDataString(backendId)}",
                    CreateClientPayload(client));
                response.EnsureSuccessStatusCode();
                return;
            }
            catch
            {
                var existing = MockData.Clients.FirstOrDefault(c => c.Id == client.Id);
                if (existing != null && !ReferenceEquals(existing, client))
                {
                    CopyClient(existing, client);
                }
            }
        }

        public async Task DeleteClientsAsync(IEnumerable<ClientRecord> clients)
        {
            try
            {
                foreach (var client in clients.Where(c => !string.IsNullOrWhiteSpace(c.BackendId) || !string.IsNullOrWhiteSpace(c.Id)))
                {
                    var backendId = client.BackendId ?? client.Id!;
                    using var response = await _clientsApiHttpClient.DeleteAsync($"api/v1/clients/{Uri.EscapeDataString(backendId)}");
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        LogService.Instance.AddLog("API Error", "Clients", "System", $"Delete client {backendId} failed: {response.StatusCode}");
                        response.EnsureSuccessStatusCode();
                    }
                }

                return;
            }
            catch (Exception ex)
            {
                LogService.Instance.AddLog("Warning", "Clients", "System", $"Delete failed, falling back: {ex.Message}");
                foreach (var client in clients.ToList())
                {
                    MockData.Clients.Remove(client);
                }
            }
        }

        public Task<List<ActivityLogEntry>> GetActivityLogsAsync() =>
            GetListAsync("api/activity-logs", () => MockData.ActivityLogs.OrderByDescending(l => l.Timestamp).ToList());

        public async Task AddActivityLogAsync(ActivityLogEntry entry)
        {
            if (!await PostAsync("api/activity-logs", entry))
            {
                MockData.ActivityLogs.Insert(0, entry);
            }
        }

        public async Task<List<AuditRecord>> GetAuditRecordsAsync(string category)
        {
            var normalized = NormalizeAuditCategory(category);
            var auditApiRoute = TryGetAuditApiRoute(normalized);
            if (auditApiRoute != null)
            {
                try
                {
                    return await TryLoadAuditRecordsFromApiAsync(auditApiRoute);
                }
                catch (Exception ex)
                {
                    LogService.Instance.AddLog("Warning", "Audit", "System", $"API failure for {normalized}, falling back to mock: {ex.Message}");
                    return MockData.AuditRecords.TryGetValue(normalized, out var records)
                        ? records
                        : new List<AuditRecord>();
                }
            }

            return await GetListAsync(
                $"api/audit-records?category={Uri.EscapeDataString(normalized)}",
                () => MockData.AuditRecords.TryGetValue(normalized, out var records) ? records : new List<AuditRecord>());
        }

        public async Task AddAuditRecordAsync(string category, AuditRecord record)
        {
            var normalized = NormalizeAuditCategory(category);
            record.ID ??= Guid.NewGuid().ToString("N");

            var auditApiRoute = TryGetAuditApiRoute(normalized);
            if (auditApiRoute != null)
            {
                try
                {
                    using var response = await _clientsApiHttpClient.PostAsJsonAsync($"api/v1/Audit/{auditApiRoute}", CreateAuditPayload(record));
                    response.EnsureSuccessStatusCode();
                    return;
                }
                catch
                {
                    // Fall through to the existing fallback path.
                }
            }

            if (!await PostAsync($"api/audit-records?category={Uri.EscapeDataString(normalized)}", record))
            {
                if (!MockData.AuditRecords.ContainsKey(normalized))
                {
                    MockData.AuditRecords[normalized] = new List<AuditRecord>();
                }

                MockData.AuditRecords[normalized].Add(record);
            }
        }

        public async Task UpdateAuditRecordAsync(string category, AuditRecord record)
        {
            var normalized = NormalizeAuditCategory(category);
            if (string.IsNullOrWhiteSpace(record?.ID))
            {
                return;
            }

            var auditApiRoute = TryGetAuditApiRoute(normalized);
            if (auditApiRoute != null)
            {
                try
                {
                    using var response = await _clientsApiHttpClient.PutAsJsonAsync(
                        $"api/v1/Audit/{auditApiRoute}/{Uri.EscapeDataString(record.ID)}",
                        CreateAuditPayload(record));
                    response.EnsureSuccessStatusCode();
                    return;
                }
                catch
                {
                    // Fall through to the existing fallback path.
                }
            }

            var updated = await TryApiAsync(async () =>
            {
                using var response = await _httpClient.PutAsJsonAsync(
                    $"api/audit-records/{Uri.EscapeDataString(record.ID)}?category={Uri.EscapeDataString(normalized)}",
                    record);
                response.EnsureSuccessStatusCode();
                return true;
            });

            if (!updated.HasValue && MockData.AuditRecords.TryGetValue(normalized, out var records))
            {
                var existing = records.FirstOrDefault(r => r.ID == record.ID);
                if (existing != null && !ReferenceEquals(existing, record))
                {
                    CopyAuditRecord(existing, record);
                }
            }
        }

        public async Task DeleteAuditRecordsAsync(string category, IEnumerable<AuditRecord> records)
        {
            var normalized = NormalizeAuditCategory(category);
            var auditApiRoute = TryGetAuditApiRoute(normalized);

            if (auditApiRoute != null)
            {
                try
                {
                    foreach (var record in records.Where(r => !string.IsNullOrWhiteSpace(r.ID)))
                    {
                        using var response = await _clientsApiHttpClient.DeleteAsync(
                            $"api/v1/Audit/{auditApiRoute}/{Uri.EscapeDataString(record.ID!)}");
                        
                        if (!response.IsSuccessStatusCode)
                        {
                            LogService.Instance.AddLog("API Error", "Audit", "System", $"Delete {auditApiRoute} {record.ID} failed: {response.StatusCode}");
                            response.EnsureSuccessStatusCode();
                        }
                    }

                    return;
                }
                catch (Exception ex)
                {
                    LogService.Instance.AddLog("Warning", "Audit", "System", $"Delete failed, falling back: {ex.Message}");
                }
            }

            var ids = records.Where(r => !string.IsNullOrWhiteSpace(r.ID)).Select(r => r.ID!).ToList();
            if (!await PostAsync($"api/audit-records/bulk-delete?category={Uri.EscapeDataString(normalized)}", ids) &&
                MockData.AuditRecords.TryGetValue(normalized, out var existingRecords))
            {
                foreach (var record in records.ToList())
                {
                    existingRecords.Remove(record);
                }
            }
        }

        public async Task<List<TaxRecord>> GetTaxRecordsAsync(string category)
        {
            var normalized = NormalizeTaxCategory(category);
            var taxApiRoute = TryGetTaxApiRoute(normalized);
            if (taxApiRoute != null)
            {
                try
                {
                    return await TryLoadTaxRecordsFromApiAsync(taxApiRoute);
                }
                catch (Exception ex)
                {
                    LogService.Instance.AddLog("Warning", "Tax", "System", $"API failure for {normalized}, falling back to mock: {ex.Message}");
                    return MockData.TaxRecords.TryGetValue(normalized, out var records) ? records : new List<TaxRecord>();
                }
            }

            return await GetListAsync(
                $"api/tax-records?category={Uri.EscapeDataString(normalized)}",
                () => MockData.TaxRecords.TryGetValue(normalized, out var records) ? records : new List<TaxRecord>());
        }

        public async Task AddTaxRecordAsync(string category, TaxRecord record)
        {
            var normalized = NormalizeTaxCategory(category);
            record.ID ??= Guid.NewGuid().ToString("N");

            var taxApiRoute = TryGetTaxApiRoute(normalized);
            if (taxApiRoute != null)
            {
                try
                {
                    using var response = await _clientsApiHttpClient.PostAsJsonAsync($"api/v1/Tax/{taxApiRoute}", CreateTaxPayload(record));
                    if (!response.IsSuccessStatusCode)
                    {
                        LogService.Instance.AddLog("API Error", "Tax", "System", $"Add {taxApiRoute} failed: {response.StatusCode}");
                    }
                    response.EnsureSuccessStatusCode();
                    return;
                }
                catch (Exception ex)
                {
                    LogService.Instance.AddLog("Warning", "Tax", "System", $"API add failed, falling back: {ex.Message}");
                }
            }

            if (!await PostAsync($"api/tax-records?category={Uri.EscapeDataString(normalized)}", record))
            {
                if (!MockData.TaxRecords.ContainsKey(normalized))
                {
                    MockData.TaxRecords[normalized] = new List<TaxRecord>();
                }

                MockData.TaxRecords[normalized].Add(record);
            }
        }

        public async Task UpdateTaxRecordAsync(string category, TaxRecord record)
        {
            var normalized = NormalizeTaxCategory(category);
            if (string.IsNullOrWhiteSpace(record?.ID))
            {
                return;
            }

            var taxApiRoute = TryGetTaxApiRoute(normalized);
            if (taxApiRoute != null)
            {
                try
                {
                    var payload = CreateTaxPayload(record);
                    var url = $"api/v1/Tax/{taxApiRoute}/{Uri.EscapeDataString(record.ID)}";
                    
                    LogService.Instance.AddLog("Debug", "Tax", "System", $"Updating {url}...");
                    
                    using var response = await _clientsApiHttpClient.PutAsJsonAsync(url, payload);
                    
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        LogService.Instance.AddLog("API Error", "Tax", "System", $"Update {taxApiRoute} {record.ID} failed: {response.StatusCode}. Response: {errorBody}");
                        response.EnsureSuccessStatusCode();
                    }
                    else
                    {
                        LogService.Instance.AddLog("Success", "Tax", "System", $"Updated {taxApiRoute} {record.ID} successfully.");
                    }
                    return;
                }
                catch (Exception ex)
                {
                    LogService.Instance.AddLog("Warning", "Tax", "System", $"API update failed, falling back: {ex.Message}");
                }
            }

            var updated = await TryApiAsync(async () =>
            {
                using var response = await _httpClient.PutAsJsonAsync(
                    $"api/tax-records/{Uri.EscapeDataString(record.ID)}?category={Uri.EscapeDataString(normalized)}",
                    record);
                response.EnsureSuccessStatusCode();
                return true;
            });

            if (!updated.HasValue && MockData.TaxRecords.TryGetValue(normalized, out var records))
            {
                var existing = records.FirstOrDefault(r => r.ID == record.ID);
                if (existing != null && !ReferenceEquals(existing, record))
                {
                    CopyTaxRecord(existing, record);
                }
            }
        }

        public async Task DeleteTaxRecordsAsync(string category, IEnumerable<TaxRecord> records)
        {
            var normalized = NormalizeTaxCategory(category);
            var taxApiRoute = TryGetTaxApiRoute(normalized);

            if (taxApiRoute != null)
            {
                try
                {
                    foreach (var record in records.Where(r => !string.IsNullOrWhiteSpace(r.ID)))
                    {
                        using var response = await _clientsApiHttpClient.DeleteAsync(
                            $"api/v1/Tax/{taxApiRoute}/{Uri.EscapeDataString(record.ID!)}");
                        
                        if (!response.IsSuccessStatusCode)
                        {
                            LogService.Instance.AddLog("API Error", "Tax", "System", $"Delete {taxApiRoute} {record.ID} failed: {response.StatusCode}");
                            response.EnsureSuccessStatusCode();
                        }
                    }

                    return;
                }
                catch (Exception ex)
                {
                    LogService.Instance.AddLog("Warning", "Tax", "System", $"Delete failed, falling back: {ex.Message}");
                }
            }

            var ids = records.Where(r => !string.IsNullOrWhiteSpace(r.ID)).Select(r => r.ID!).ToList();
            if (!await PostAsync($"api/tax-records/bulk-delete?category={Uri.EscapeDataString(normalized)}", ids) &&
                MockData.TaxRecords.TryGetValue(normalized, out var existingRecords))
            {
                foreach (var record in records.ToList())
                {
                    existingRecords.Remove(record);
                }
            }
        }

        private async Task<List<T>> GetListAsync<T>(string route, Func<List<T>> fallback)
        {
            var result = await TryApiAsync(async () =>
            {
                using var response = await _httpClient.GetAsync(route);
                if (response.IsSuccessStatusCode)
                {
                    return await response.Content.ReadFromJsonAsync<List<T>>(JsonOptions);
                }
                
                LogService.Instance.AddLog("API Error", "Service", "System", $"Fetch {route} failed: {response.StatusCode}");
                return null;
            });

            if (result == null)
            {
                LogService.Instance.AddLog("Warning", "Service", "System", $"Returning mock data for {route}");
                return fallback();
            }

            return result;
        }

        private async Task<bool> PostAsync<T>(string route, T payload)
        {
            var result = await TryApiAsync(async () =>
            {
                using var response = await _httpClient.PostAsJsonAsync(route, payload);
                if (!response.IsSuccessStatusCode)
                {
                    LogService.Instance.AddLog("API Error", "Service", "System", $"Post to {route} failed: {response.StatusCode}");
                }
                response.EnsureSuccessStatusCode();
                return true;
            });

            return result.HasValue && result.Value;
        }

        private async Task<T?> TryApiAsync<T>(Func<Task<T?>> action)
        {
            try
            {
                return await action();
            }
            catch
            {
                if (_settings.Backend.UseMockFallback)
                {
                    return default;
                }

                throw;
            }
        }

        private async Task<bool?> TryApiAsync(Func<Task<bool>> action)
        {
            try
            {
                return await action();
            }
            catch
            {
                if (_settings.Backend.UseMockFallback)
                {
                    return null;
                }

                throw;
            }
        }

        private static Uri CreateBaseUri(string? baseUrl)
        {
            var value = string.IsNullOrWhiteSpace(baseUrl) ? "http://localhost:5561/" : baseUrl.Trim();
            if (!value.EndsWith("/", StringComparison.Ordinal))
            {
                value += "/";
            }

            return new Uri(value, UriKind.Absolute);
        }

        private static string NormalizeAuditCategory(string category) => category switch
        {
            "Import and Export Clearance" => "Import / Export",
            "Audit Others" => "Others",
            _ => category
        };

        private async Task<List<TaxRecord>> TryLoadTaxRecordsFromApiAsync(string taxApiRoute)
        {
            try
            {
                using var response = await _clientsApiHttpClient.GetAsync($"api/v1/Tax/{taxApiRoute}?Page=1&Limit=1000");
                if (!response.IsSuccessStatusCode)
                {
                    LogService.Instance.AddLog("API Error", "Tax", "System", $"Fetch {taxApiRoute} failed: {response.StatusCode}");
                    response.EnsureSuccessStatusCode();
                }

                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);

                var records = ExtractCollection(document.RootElement)
                    .Select(MapTaxRecord)
                    .Where(record => !string.IsNullOrWhiteSpace(record.ID) || !string.IsNullOrWhiteSpace(record.ClientName))
                    .ToList();

                if (records.Count == 0)
                {
                    var snippet = json.Length > 500 ? json.Substring(0, 500) : json;
                    LogService.Instance.AddLog("Info", "Tax", "System", $"API returned 0 records for {taxApiRoute}. JSON: {snippet}");
                }

                return records;
            }
            catch (Exception ex)
            {
                LogService.Instance.AddLog("API Error", "Tax", "System", $"Exception loading {taxApiRoute}: {ex.Message}");
                throw;
            }
        }

        private static TaxRecord MapTaxRecord(JsonElement element)
        {
            return new TaxRecord
            {
                ID = GetString(element, "id", "Id", "taxId", "recordId", "backend_id"),
                ClientName = GetString(element, "clientName", "name", "companyName", "legalName", "ClientName"),
                ClientNameSub = GetString(element, "clientCode", "clientId", "client_code", "ClientNameSub"),
                DINNo = GetString(element, "dinNo", "tinNo", "referenceNo", "DINNo", "tin"),
                TaxPeriod = GetString(element, "taxPeriod", "period", "TaxPeriod", "tax_period"),
                Status = GetString(element, "status", "paymentStatus", "Status", "state"),
                Branch = GetString(element, "branchName", "branch", "Branch", "location"),
                Date = GetDate(element, "date", "createdAt", "updatedAt", "Date") ?? DateTime.Now,
                Notes = GetString(element, "notes", "description", "Notes", "comment")
            };
        }

        private static object CreateTaxPayload(TaxRecord record) => new
        {
            id = record.ID,
            clientName = record.ClientName,
            clientCode = record.ClientNameSub,
            dinNo = record.DINNo,
            taxPeriod = record.TaxPeriod,
            status = record.Status,
            branchName = record.Branch,
            date = record.Date == default ? DateTime.Now : record.Date,
            notes = record.Notes
        };

        private static string? TryGetTaxApiRoute(string category) => category switch
        {
            "Corporate Income Tax (CIT)" or "CIT" => "filings",
            "Individual Income Tax (IIT)" or "IIT" => "filings",
            "Social Security Contribution Levy (SSCL)" or "SSCL" => "filings",
            "Value Added Tax (VAT)" or "VAT" => "filings",
            "Withholding Tax (WHT)" or "WHT" => "filings",
            "Tax Others" or "Others" => "records",
            _ => "records" // Default to records for other tax types
        };

        private static string NormalizeTaxCategory(string category) => category;

        private static TeamMember MapTeamMember(JsonElement element)
        {
            return new TeamMember
            {
                Id = GetString(element, "id", "userId", "Id", "userId"),
                Username = GetString(element, "username", "name", "Name", "userName"),
                Email = GetString(element, "email", "Email", "userEmail"),
                Phone = GetString(element, "phone", "Phone", "phoneNumber", "mobile"),
                Branch = GetString(element, "branchName", "branch", "Branch", "location"),
                Role = GetString(element, "role", "Role", "userRole"),
                Status = GetString(element, "status", "Status", "state"),
                LogoUrl = GetString(element, "logoUrl", "logo", "profileImage", "avatar"),
                CreatedAt = GetDate(element, "createdAt", "date", "Date", "created_at") ?? DateTime.Now
            };
        }

        private static object CreateUserPayload(TeamMember member) => new
        {
            username = member.Username,
            email = member.Email,
            phone = member.Phone,
            role = member.Role,
            branchName = member.Branch,
            status = member.Status ?? "Active"
        };

        private static void CopyTeamMember(TeamMember target, TeamMember source)
        {
            target.Username = source.Username;
            target.Email = source.Email;
            target.Phone = source.Phone;
            target.Branch = source.Branch;
            target.Role = source.Role;
            target.Status = source.Status;
            target.LogoUrl = source.LogoUrl;
            target.CreatedAt = source.CreatedAt;
        }

        private static void CopyClient(ClientRecord target, ClientRecord source)
        {
            target.Name = source.Name;
            target.Email = source.Email;
            target.Phone = source.Phone;
            target.Branch = source.Branch;
            target.BackendId = source.BackendId;
            target.Category = source.Category;
            target.Status = source.Status;
            target.TotalRevenue = source.TotalRevenue;
            target.DueAmount = source.DueAmount;
        }

        private async Task<List<ClientRecord>> LoadClientsFromClientsApiAsync()
        {
            try
            {
                using var response = await _clientsApiHttpClient.GetAsync("api/v1/clients");
                if (!response.IsSuccessStatusCode)
                {
                    LogService.Instance.AddLog("API Error", "Clients", "System", $"Fetch clients failed: {response.StatusCode}");
                    response.EnsureSuccessStatusCode();
                }

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var document = await JsonDocument.ParseAsync(stream);

                var clients = ExtractCollection(document.RootElement)
                    .Select(MapClientRecord)
                    .Where(client => !string.IsNullOrWhiteSpace(client.Id) || !string.IsNullOrWhiteSpace(client.Name))
                    .ToList();

                await PopulateRevenueSummariesAsync(clients);
                return clients;
            }
            catch (Exception ex)
            {
                LogService.Instance.AddLog("API Error", "Clients", "System", $"Exception loading clients: {ex.Message}");
                throw;
            }
        }

        private async Task PopulateRevenueSummariesAsync(List<ClientRecord> clients)
        {
            var tasks = clients
                .Where(client => !string.IsNullOrWhiteSpace(client.Id))
                .Select(PopulateRevenueSummaryAsync);

            await Task.WhenAll(tasks);
        }

        private async Task PopulateRevenueSummaryAsync(ClientRecord client)
        {
            try
            {
                var backendId = client.BackendId ?? client.Id;
                if (string.IsNullOrWhiteSpace(backendId))
                {
                    return;
                }

                using var response = await _clientsApiHttpClient.GetAsync($"api/v1/clients/{Uri.EscapeDataString(backendId)}/revenue-summary");
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var document = await JsonDocument.ParseAsync(stream);
                var root = document.RootElement;

                if (TryGetDecimal(root, out var totalRevenue, "totalRevenue", "revenue", "total_revenue", "grossRevenue"))
                {
                    client.TotalRevenue = totalRevenue;
                }

                if (TryGetDecimal(root, out var dueAmount, "dueAmount", "outstanding", "outstandingBalance", "due_amount"))
                {
                    client.DueAmount = dueAmount;
                }
            }
            catch
            {
                // Preserve defaults when the revenue endpoint is unavailable for a record.
            }
        }

        private static IEnumerable<JsonElement> ExtractCollection(JsonElement root)
        {
            if (root.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in root.EnumerateArray()) yield return item;
                yield break;
            }

            var collectionNames = new[] { "data", "items", "results", "clients", "records", "auditRecords", "audit_records", "payload", "result", "assurance", "forensic", "internal", "value", "list", "entries" };

            // 1. Check top-level properties for an array
            foreach (var name in collectionNames)
            {
                if (TryGetProperty(root, out var prop, name))
                {
                    if (prop.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in prop.EnumerateArray()) yield return item;
                        yield break;
                    }
                }
            }

            // 2. Check one level deeper (e.g., { "data": { "items": [...] } })
            foreach (var name in new[] { "data", "result", "payload", "assurance" })
            {
                if (TryGetProperty(root, out var nested, name) && nested.ValueKind == JsonValueKind.Object)
                {
                    foreach (var subName in collectionNames)
                    {
                        if (TryGetProperty(nested, out var collection, subName) && collection.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var item in collection.EnumerateArray()) yield return item;
                            yield break;
                        }
                    }
                }
            }
        }

        private static ClientRecord MapClientRecord(JsonElement element)
        {
            var client = new ClientRecord
            {
                // Broaden field mapping to handle various backend naming conventions.
                BackendId = GetString(element, "id", "clientId", "client_id", "backend_id", "BackendId"),
                Id = GetString(element, "clientCode", "id", "clientId", "client_id", "code", "ClientCode"),
                Name = GetString(element, "clientName", "name", "companyName", "legalName", "ClientName", "company_name"),
                Email = GetString(element, "email", "contactEmail", "primaryEmail", "Email"),
                Phone = GetString(element, "phone", "mobile", "phoneNumber", "contactNumber", "Phone"),
                Branch = GetString(element, "branchName", "branch", "location", "BranchName"),
                Category = NormalizeCategory(GetString(element, "category", "type", "clientType", "Category")),
                Status = NormalizeStatus(GetString(element, "status", "state", "Status"))
            };

            if (TryGetDecimal(element, out var totalRevenue, "totalRevenue", "revenue", "total_revenue", "grossRevenue", "TotalRevenue"))
            {
                client.TotalRevenue = totalRevenue;
            }

            if (TryGetDecimal(element, out var dueAmount, "outstandingBalance", "dueAmount", "outstanding", "due_amount", "DueAmount"))
            {
                client.DueAmount = dueAmount;
            }

            if (string.IsNullOrWhiteSpace(client.Status) && TryGetBoolean(element, out var isActive, "isActive", "active"))
            {
                client.Status = isActive ? "Active" : "Inactive";
            }

            client.Category ??= "SME";
            client.Status ??= "Active";
            return client;
        }

        private static object CreateClientPayload(ClientRecord client) => new
        {
            clientName = client.Name,
            email = client.Email,
            phone = client.Phone,
            status = client.Status,
            branchId = (string?)null
        };

        private async Task<bool> TryLoginClientsApiAsync(string email, string password)
        {
            try
            {
                using var response = await _clientsApiHttpClient.PostAsJsonAsync("api/v1/auth/login", new { email, password });
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync();
                using var document = await JsonDocument.ParseAsync(stream);

                if (!TryGetProperty(document.RootElement, out var data, "data"))
                {
                    return false;
                }

                var token = GetString(data, "token");
                if (string.IsNullOrWhiteSpace(token))
                {
                    return false;
                }

                _clientsApiHttpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                if (TryGetProperty(data, out var user, "user"))
                {
                    CurrentUser = new TeamMember
                    {
                        Id = GetString(user, "id"),
                        Username = GetString(user, "username"),
                        Email = GetString(user, "email"),
                        Role = GetString(user, "role"),
                        Branch = GetString(user, "branchName"),
                        CreatedAt = DateTime.Now
                    };
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string? GetString(JsonElement element, params string[] names)
        {
            foreach (var name in names)
            {
                if (TryGetProperty(element, out var property, name))
                {
                    if (property.ValueKind == JsonValueKind.String)
                    {
                        return property.GetString();
                    }

                    if (property.ValueKind != JsonValueKind.Null && property.ValueKind != JsonValueKind.Undefined)
                    {
                        return property.ToString();
                    }
                }
            }

            return null;
        }

        private static bool TryGetDecimal(JsonElement element, out decimal value, params string[] names)
        {
            foreach (var name in names)
            {
                if (TryGetProperty(element, out var property, name))
                {
                    if (property.ValueKind == JsonValueKind.Number && property.TryGetDecimal(out value))
                    {
                        return true;
                    }

                    if (property.ValueKind == JsonValueKind.String && decimal.TryParse(property.GetString(), out value))
                    {
                        return true;
                    }
                }
            }

            value = 0m;
            return false;
        }

        private static bool TryGetBoolean(JsonElement element, out bool value, params string[] names)
        {
            foreach (var name in names)
            {
                if (TryGetProperty(element, out var property, name))
                {
                    if (property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False)
                    {
                        value = property.GetBoolean();
                        return true;
                    }

                    if (property.ValueKind == JsonValueKind.String && bool.TryParse(property.GetString(), out value))
                    {
                        return true;
                    }
                }
            }

            value = false;
            return false;
        }

        private static bool TryGetProperty(JsonElement element, out JsonElement property, string name)
        {
            foreach (var candidate in element.EnumerateObject())
            {
                if (string.Equals(candidate.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    property = candidate.Value;
                    return true;
                }
            }

            property = default;
            return false;
        }

        private static string NormalizeCategory(string? value) => value?.Trim() switch
        {
            null or "" => "SME",
            "Corporate" => "Corporate",
            "SME" => "SME",
            var other => other
        };

        private static string NormalizeStatus(string? value) => value?.Trim() switch
        {
            null or "" => "Active",
            "Active" => "Active",
            "Inactive" => "Inactive",
            "Deleted" => "Inactive",
            var other => other
        };

        private async Task<List<AuditRecord>> TryLoadAuditRecordsFromApiAsync(string auditApiRoute)
        {
            try
            {
                // Use PascalCase for Page and Limit as per Swagger.
                using var response = await _clientsApiHttpClient.GetAsync($"api/v1/Audit/{auditApiRoute}?Page=1&Limit=1000");
                if (!response.IsSuccessStatusCode)
                {
                    LogService.Instance.AddLog("API Error", "Audit", "System", $"Fetch {auditApiRoute} failed: {response.StatusCode}");
                    response.EnsureSuccessStatusCode();
                }

                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);

                var records = ExtractCollection(document.RootElement)
                    .Select(MapAuditRecord)
                    .Where(record => !string.IsNullOrWhiteSpace(record.ID) || !string.IsNullOrWhiteSpace(record.ClientName))
                    .ToList();

                if (records.Count == 0)
                {
                    var snippet = json.Length > 100 ? json.Substring(0, 100) : json;
                    LogService.Instance.AddLog("Info", "Audit", "System", $"API returned 0 records for {auditApiRoute}. JSON: {snippet}");
                }

                return records;
            }
            catch (Exception ex)
            {
                LogService.Instance.AddLog("API Error", "Audit", "System", $"Exception loading {auditApiRoute}: {ex.Message}");
                throw;
            }
        }

        private static AuditRecord MapAuditRecord(JsonElement element)
        {
            var description = GetString(element, "description", "Assignment", "assignment", "Notes", "notes");

            return new AuditRecord
            {
                // Broaden field mapping to handle various backend naming conventions.
                ID = GetString(element, "id", "Id", "ID", "auditId", "audit_id", "recordId"),
                ClientName = GetString(element, "clientName", "name", "companyName", "ClientName", "client_name"),
                Date = GetDate(element, "date", "Date", "createdAt", "created_at") ?? DateTime.Now,
                Branch = GetString(element, "branchName", "branch", "Branch", "location"),
                Assignment = description,
                Notes = description,
                Process = GetString(element, "process", "status", "state", "Process"),
                PaymentStatus = GetString(element, "paymentStatus", "payment_status", "PaymentStatus"),
                PaymentOption = GetString(element, "paymentOption", "paymentMode", "PaymentOption", "payment_mode")
            };
        }

        private static object CreateAuditPayload(AuditRecord record) => new
        {
            clientName = record.ClientName,
            date = record.Date == default ? DateTime.Now.ToString("yyyy-MM-dd") : record.Date.ToString("yyyy-MM-dd"),
            branchId = (string?)null,
            description = string.IsNullOrWhiteSpace(record.Assignment) ? record.Notes : record.Assignment,
            assignedToId = (string?)null,
            process = record.Process,
            clientId = (string?)null,
            paymentStatus = record.PaymentStatus,
            paymentMode = record.PaymentOption,
            serviceFee = 0,
            govFee = 0,
            totalFee = 0,
            paidAmount = 0,
            paymentOption = record.PaymentOption,
            status = "Active"
        };

        private static DateTime? GetDate(JsonElement element, params string[] names)
        {
            foreach (var name in names)
            {
                if (TryGetProperty(element, out var property, name))
                {
                    if (property.ValueKind == JsonValueKind.String && DateTime.TryParse(property.GetString(), out var parsed))
                    {
                        return parsed;
                    }
                }
            }

            return null;
        }

        private static string? TryGetAuditApiRoute(string category) => category switch
        {
            "Audit & Assurance" or "assurance" => "assurance",
            "Forensic Audit" or "forensic" => "forensic",
            "Internal Audit" or "internal" => "internal",
            "Management Accountings" or "management-accounts" => "management-accounts",
            "Tax Accountings" or "tax-accounts" => "tax-accounts",
            "Internal Control Systems & Outsourcing" or "internal-control" => "internal-control",
            "BOI" or "boi" => "boi",
            "EPF / ETF" or "epf-etf" => "epf-etf",
            "Company Registration" or "company-registration" => "company-registration",
            "Trade License" or "trade-license" => "trade-license",
            "Trade Mark" or "trade-mark" => "trade-mark",
            "Import / Export" or "import-export" => "import-export",
            "Business Plan and Asset Valuation Consulting" or "business-plan" => "business-plan",
            "HR and Management Consulting" or "hr-consulting" => "hr-consulting",
            "Others" or "others" => "others",
            _ => null
        };

        private static void CopyAuditRecord(AuditRecord target, AuditRecord source)
        {
            target.Date = source.Date;
            target.ClientName = source.ClientName;
            target.Company = source.Company;
            target.PaymentStatus = source.PaymentStatus;
            target.Process = source.Process;
            target.PaymentOption = source.PaymentOption;
            target.Assignment = source.Assignment;
            target.Branch = source.Branch;
            target.NoOfStaffs = source.NoOfStaffs;
            target.Country = source.Country;
            target.Notes = source.Notes;
            target.Period = source.Period;
            target.TIN = source.TIN;
            target.DirectorID = source.DirectorID;
            target.InvestmentValue = source.InvestmentValue;
            target.CountryAddress = source.CountryAddress;
            target.Code = source.Code;
            target.Address = source.Address;
            target.Email = source.Email;
            target.PhoneNo = source.PhoneNo;
            target.Objective = source.Objective;
            target.Description = source.Description;
            target.DirectorsList = source.DirectorsList;
            target.SecretariesList = source.SecretariesList;
            target.ShareholdersList = source.ShareholdersList;
            target.OthersList = source.OthersList;
            target.RegistrationDocuments = source.RegistrationDocuments;
            target.SourceDocuments = source.SourceDocuments;
            target.StaffList = source.StaffList;
            target.CurrentStep = source.CurrentStep;
        }

        private static void CopyTaxRecord(TaxRecord target, TaxRecord source)
        {
            target.ClientName = source.ClientName;
            target.ClientNameSub = source.ClientNameSub;
            target.DINNo = source.DINNo;
            target.TaxPeriod = source.TaxPeriod;
            target.Status = source.Status;
            target.Branch = source.Branch;
            target.Date = source.Date;
            target.Notes = source.Notes;
        }
    }
}
