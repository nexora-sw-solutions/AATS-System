using Microsoft.AspNetCore.Mvc;
using AATS.Domain.Entities;
using AATS.Application.Common.Interfaces;
using AATS.API.Models;

namespace AATS.API.Controllers
{
    public class BranchesController : BaseApiController<Branch>
    {
        public BranchesController(IRepository<Branch> repository) : base(repository) { }
    }

    public class UsersController : BaseApiController<User>
    {
        private readonly IAuthService _authService;
        public UsersController(IRepository<User> repository, IAuthService authService) : base(repository) 
        { 
            _authService = authService;
        }

        public override async Task<ActionResult<ApiResponse<PaginatedResult<User>>>> GetAll()
        {
            var list = await _repository.GetWithInclude(u => u.Branch);
            
            var result = new PaginatedResult<User>
            {
                Items = list.ToList(),
                TotalCount = list.Count()
            };
            return Ok(ApiResponse<PaginatedResult<User>>.Ok(result));
        }

        [HttpPut("{id}")]
        public override async Task<ActionResult<ApiResponse<User>>> Update(Guid id, [FromBody] User entity)
        {
            if (id != entity.Id) return BadRequest(ApiResponse<User>.Failure("INVALID_ID", "ID mismatch"));

            var existingUser = await _repository.GetByIdAsync(id);
            if (existingUser == null) return NotFound(ApiResponse<User>.Failure("NOT_FOUND", "User not found"));

            // Update allowed fields
            existingUser.Username = entity.Username;
            existingUser.Email = entity.Email;
            existingUser.Phone = entity.Phone;
            existingUser.BranchId = entity.BranchId;
            existingUser.Role = entity.Role;
            existingUser.IsActive = entity.IsActive;
            existingUser.UpdatedAt = DateTime.UtcNow;

            // Only update password if a new one is provided
            if (!string.IsNullOrWhiteSpace(entity.Password))
            {
                // Validate current password
                if (string.IsNullOrWhiteSpace(entity.CurrentPassword))
                {
                    return BadRequest(ApiResponse<User>.Failure("VALIDATION_ERROR", "Current password is required to change password."));
                }

                if (!_authService.VerifyPassword(entity.CurrentPassword, existingUser.PasswordHash))
                {
                    return BadRequest(ApiResponse<User>.Failure("VALIDATION_ERROR", "Invalid current password."));
                }

                existingUser.PasswordHash = _authService.HashPassword(entity.Password);
            }

            await _repository.UpdateAsync(existingUser);
            await _repository.SaveChangesAsync();
            return Ok(ApiResponse<User>.Ok(existingUser));
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
            return await base.Create(entity);
        }

        public override async Task<ActionResult<ApiResponse<PaginatedResult<Client>>>> GetAll()
        {
            var list = await _repository.GetWithInclude(c => c.Branch);
            
            var result = new PaginatedResult<Client>
            {
                Items = list.ToList(),
                TotalCount = list.Count()
            };
            return Ok(ApiResponse<PaginatedResult<Client>>.Ok(result));
        }

        [HttpPut("{id}")]
        public override async Task<ActionResult<ApiResponse<Client>>> Update(Guid id, [FromBody] Client entity)
        {
            if (id != entity.Id) return BadRequest(ApiResponse<Client>.Failure("INVALID_ID", "ID mismatch"));

            var existingClient = await _repository.GetByIdAsync(id);
            if (existingClient == null) return NotFound(ApiResponse<Client>.Failure("NOT_FOUND", "Client not found"));

            // Update allowed fields
            existingClient.Name = entity.Name;
            existingClient.Email = entity.Email;
            existingClient.Phone = entity.Phone;
            existingClient.Status = entity.Status;
            Console.WriteLine($"[DEBUG] Updating client {id}. New Status: {entity.Status}");
            existingClient.BranchId = entity.BranchId;
            existingClient.Category = entity.Category;
            existingClient.TotalRevenue = entity.TotalRevenue;
            existingClient.OutstandingBalance = entity.OutstandingBalance;
            existingClient.UpdatedAt = DateTime.UtcNow;

            await _repository.UpdateAsync(existingClient);
            await _repository.SaveChangesAsync();
            return Ok(ApiResponse<Client>.Ok(existingClient));
        }
    }
}
