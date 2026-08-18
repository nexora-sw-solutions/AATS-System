using Microsoft.AspNetCore.Mvc;
using AATS.Domain.Entities;
using AATS.Application.Common.Interfaces;
using AATS.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using AATS.Infrastructure.Persistence;

namespace AATS.API.Controllers
{
    public class UsersController : BaseApiController<User>
    {
        public UsersController(IRepository<User> repository) : base(repository) { }

        public override async Task<ActionResult<ApiResponse<PaginatedResult<User>>>> GetAll([FromQuery] bool enrich = true, [FromQuery] bool includeDeleted = true)
        {
            var list = await _repository.GetWithInclude(u => u.Branch);
            var now = DateTime.UtcNow;
            var toPurge = list.Where(u => u.IsDeleted && u.DeletedAt.HasValue && (now - u.DeletedAt.Value).TotalDays >= 30).ToList();
            if (toPurge.Any())
            {
                foreach (var p in toPurge) await _repository.DeleteAsync(p);
                await _repository.SaveChangesAsync();
                list = list.Except(toPurge).ToList();
            }

            var items = includeDeleted ? list : list.Where(u => !u.IsDeleted).ToList();
            var filteredList = await FilterListForCurrentUserAsync(items);

            var result = new PaginatedResult<User>
            {
                Items = filteredList,
                TotalCount = filteredList.Count
            };
            return Ok(ApiResponse<PaginatedResult<User>>.Ok(result));
        }

        public override async Task<ActionResult<ApiResponse<User>>> GetById(Guid id)
        {
            var users = await _repository.GetWithInclude(u => u.Branch);
            var user = users.FirstOrDefault(u => u.Id == id);
            
            if (user == null || !await CheckUserAccessAsync(user))
            {
                return NotFound(ApiResponse<User>.Failure("NOT_FOUND", "User not found"));
            }

            return Ok(ApiResponse<User>.Ok(user));
        }

        [HttpPut("{id}")]
        public override async Task<ActionResult<ApiResponse<User>>> Update(Guid id, [FromBody] User entity)
        {
            if (id != entity.Id) return BadRequest(ApiResponse<User>.Failure("INVALID_ID", "ID mismatch"));

            var existingUser = await _repository.GetByIdAsync(id);
            if (existingUser == null || !await CheckUserAccessAsync(existingUser))
                return NotFound(ApiResponse<User>.Failure("NOT_FOUND", "User not found"));

            existingUser.Username = entity.Username;
            existingUser.Email = entity.Email;
            existingUser.Phone = entity.Phone;
            existingUser.Role = entity.Role;
            existingUser.BranchId = entity.BranchId;

            if (!string.IsNullOrWhiteSpace(entity.Password))
            {
                existingUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(entity.Password);
            }

            await _repository.UpdateAsync(existingUser);
            await _repository.SaveChangesAsync();

            return Ok(ApiResponse<User>.Ok(existingUser));
        }
    }

    public class BranchesController : BaseApiController<Branch>
    {
        public BranchesController(IRepository<Branch> repository) : base(repository) { }

        public override async Task<ActionResult<ApiResponse<PaginatedResult<Branch>>>> GetAll([FromQuery] bool enrich = true, [FromQuery] bool includeDeleted = true)
        {
            var (isStaff, userBranchId) = await GetCurrentUserRoleAndBranchAsync();
            var list = await _repository.GetAllAsync();
            var items = includeDeleted ? list : list.Where(b => !b.IsDeleted).ToList();
            
            if (isStaff && userBranchId.HasValue)
            {
                items = items.Where(b => b.Id == userBranchId.Value).ToList();
            }

            var branchList = items.ToList();
            var result = new PaginatedResult<Branch>
            {
                Items = branchList,
                TotalCount = branchList.Count
            };
            return Ok(ApiResponse<PaginatedResult<Branch>>.Ok(result));
        }
    }

    public class ClientsController : BaseApiController<Client>
    {
        public ClientsController(IRepository<Client> repository) : base(repository) { }

        public override async Task<ActionResult<ApiResponse<Client>>> Create(Client entity)
        {
            // Set current user as creator
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                entity.CreatedBy = userId;
            }

            if (string.IsNullOrWhiteSpace(entity.ClientCode))
            {
                var allClients = await _repository.GetAllAsync();
                var maxCode = allClients
                    .Select(c => c.ClientCode)
                    .Where(c => !string.IsNullOrEmpty(c) && c.StartsWith("CLT-"))
                    .OrderByDescending(c => c)
                    .FirstOrDefault();

                int nextNum = 1;
                if (!string.IsNullOrEmpty(maxCode))
                {
                    var parts = maxCode.Split('-');
                    if (parts.Length > 1 && int.TryParse(parts[1], out int currentMax))
                    {
                        nextNum = currentMax + 1;
                    }
                }
                entity.ClientCode = $"CLT-{nextNum:D3}";
            }

            // Fallback safety check for BranchId foreign key constraint
            var dbContext = HttpContext.RequestServices.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
            if (dbContext != null)
            {
                if (!entity.BranchId.HasValue || entity.BranchId.Value == Guid.Empty || !await dbContext.Branches.AnyAsync(b => b.Id == entity.BranchId.Value))
                {
                    var defaultBranch = await dbContext.Branches.FirstOrDefaultAsync();
                    if (defaultBranch != null)
                    {
                        entity.BranchId = defaultBranch.Id;
                    }
                }
            }

            var response = await base.Create(entity);
            if (response.Result is ObjectResult objRes && objRes.Value is ApiResponse<Client> apiResp && apiResp.Data != null && dbContext != null)
            {
                entity.Id = apiResp.Data.Id;
                await SaveClientDocumentsAsync(entity, dbContext);
                await EnrichClientDocumentsAsync(apiResp.Data, dbContext);
            }

            return response;
        }

        public override async Task<ActionResult<ApiResponse<PaginatedResult<Client>>>> GetAll([FromQuery] bool enrich = true, [FromQuery] bool includeDeleted = true)
        {
            var list = await _repository.GetWithInclude(c => c.Branch);
            var now = DateTime.UtcNow;
            var toPurge = list.Where(c => c.IsDeleted && c.DeletedAt.HasValue && (now - c.DeletedAt.Value).TotalDays >= 30).ToList();
            if (toPurge.Any())
            {
                foreach (var p in toPurge) await _repository.DeleteAsync(p);
                await _repository.SaveChangesAsync();
                list = list.Except(toPurge).ToList();
            }

            var items = includeDeleted ? list : list.Where(c => !c.IsDeleted).ToList();
            var filteredList = await FilterListForCurrentUserAsync(items);

            var dbContext = HttpContext.RequestServices.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
            if (dbContext != null)
            {
                foreach (var client in filteredList)
                {
                    await EnrichClientDocumentsAsync(client, dbContext);
                }
            }
            
            var result = new PaginatedResult<Client>
            {
                Items = filteredList,
                TotalCount = filteredList.Count
            };
            return Ok(ApiResponse<PaginatedResult<Client>>.Ok(result));
        }

        public override async Task<ActionResult<ApiResponse<Client>>> GetById(Guid id)
        {
            var response = await base.GetById(id);
            if (response.Result is OkObjectResult okRes && okRes.Value is ApiResponse<Client> apiResp && apiResp.Data != null)
            {
                var dbContext = HttpContext.RequestServices.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
                if (dbContext != null)
                {
                    await EnrichClientDocumentsAsync(apiResp.Data, dbContext);
                }
            }
            return response;
        }

        [HttpPut("{id}")]
        public override async Task<ActionResult<ApiResponse<Client>>> Update(Guid id, [FromBody] Client entity)
        {
            if (id != entity.Id) return BadRequest(ApiResponse<Client>.Failure("INVALID_ID", "ID mismatch"));

            var existingClient = await _repository.GetByIdAsync(id);
            if (existingClient == null || !await CheckUserAccessAsync(existingClient)) 
                return NotFound(ApiResponse<Client>.Failure("NOT_FOUND", "Client not found"));

            var (isStaff, branchId) = await GetCurrentUserRoleAndBranchAsync();
            if (isStaff && branchId.HasValue)
            {
                entity.BranchId = branchId.Value;
            }

            // Update allowed fields
            existingClient.Name = entity.Name;
            existingClient.Email = entity.Email;
            existingClient.Phone = entity.Phone;
            existingClient.Status = entity.Status;
            existingClient.BranchId = entity.BranchId;
            existingClient.Category = entity.Category;
            existingClient.LogoStorageKey = entity.LogoStorageKey;
            existingClient.TotalRevenue = entity.TotalRevenue;
            existingClient.OutstandingBalance = entity.OutstandingBalance;
            existingClient.Notes = entity.Notes;
            existingClient.UpdatedAt = DateTime.UtcNow;

            existingClient.BrAttachments = entity.BrAttachments;
            existingClient.TinAttachments = entity.TinAttachments;
            existingClient.Form01Attachments = entity.Form01Attachments;
            existingClient.ArticleOfAssociationAttachments = entity.ArticleOfAssociationAttachments;
            existingClient.NicAttachments = entity.NicAttachments;

            await _repository.UpdateAsync(existingClient);
            await _repository.SaveChangesAsync();

            var dbContext = HttpContext.RequestServices.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
            if (dbContext != null)
            {
                await SaveClientDocumentsAsync(existingClient, dbContext);
                await EnrichClientDocumentsAsync(existingClient, dbContext);
            }

            // Add activity logging
            var recordService = HttpContext.RequestServices.GetService(typeof(IRecordService)) as IRecordService;
            if (recordService != null && dbContext != null)
            {
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                Guid? userId = null;
                Guid? logBranchId = null;
                if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
                {
                    userId = parsedUserId;
                    var userObj = await dbContext.Users.FindAsync(userId);
                    logBranchId = userObj?.BranchId;
                }
                await recordService.LogActivityAsync(userId, logBranchId, "UPDATE", "Client", existingClient.Id, $"Updated Client {existingClient.ClientCode}");
            }

            return Ok(ApiResponse<Client>.Ok(existingClient));
        }

        private async Task SaveClientDocumentsAsync(Client entity, ApplicationDbContext dbContext)
        {
            if (dbContext == null || entity == null || entity.Id == Guid.Empty) return;

            var existingDocs = await dbContext.SourceDocuments
                .Where(d => d.RecordId == entity.Id && d.RecordType == "Client")
                .ToListAsync();

            if (existingDocs.Any())
            {
                dbContext.SourceDocuments.RemoveRange(existingDocs);
            }

            var allDocs = new List<(List<SourceDocument>? docs, string cat)>
            {
                (entity.BrAttachments, "BR"),
                (entity.TinAttachments, "TIN"),
                (entity.Form01Attachments, "Form01"),
                (entity.ArticleOfAssociationAttachments, "ArticleOfAssociation"),
                (entity.NicAttachments, "NIC")
            };

            foreach (var (docList, category) in allDocs)
            {
                if (docList == null) continue;
                foreach (var doc in docList)
                {
                    if (string.IsNullOrWhiteSpace(doc.Url) && string.IsNullOrWhiteSpace(doc.FileName)) continue;
                    dbContext.SourceDocuments.Add(new SourceDocument
                    {
                        RecordId = entity.Id,
                        RecordType = "Client",
                        AttachmentCategory = category,
                        FileName = string.IsNullOrWhiteSpace(doc.FileName) ? System.IO.Path.GetFileName(doc.Url ?? "") : doc.FileName,
                        Url = doc.Url ?? string.Empty,
                        Description = doc.Description ?? $"{category} Document",
                        FileSize = doc.FileSize,
                        FileType = doc.FileType
                    });
                }
            }
            await dbContext.SaveChangesAsync();
        }

        private async Task EnrichClientDocumentsAsync(Client client, ApplicationDbContext dbContext)
        {
            if (dbContext == null || client == null || client.Id == Guid.Empty) return;

            var docs = await dbContext.SourceDocuments
                .Where(d => d.RecordId == client.Id && d.RecordType == "Client")
                .ToListAsync();

            client.BrAttachments = docs.Where(d => d.AttachmentCategory == "BR").ToList();
            client.TinAttachments = docs.Where(d => d.AttachmentCategory == "TIN").ToList();
            client.Form01Attachments = docs.Where(d => d.AttachmentCategory == "Form01").ToList();
            client.ArticleOfAssociationAttachments = docs.Where(d => d.AttachmentCategory == "ArticleOfAssociation").ToList();
            client.NicAttachments = docs.Where(d => d.AttachmentCategory == "NIC").ToList();
        }
    }
}
