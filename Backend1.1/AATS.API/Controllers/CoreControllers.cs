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
        public override async Task<IActionResult> Update(Guid id, [FromBody] User entity)
        {
            if (id != entity.Id) return BadRequest();

            var existingUser = await _repository.GetByIdAsync(id);
            if (existingUser == null) return NotFound();

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
                    return BadRequest(ApiResponse<string>.Failure("VALIDATION_ERROR", "Current password is required to change password."));
                }

                if (!_authService.VerifyPassword(entity.CurrentPassword, existingUser.PasswordHash))
                {
                    return BadRequest(ApiResponse<string>.Failure("VALIDATION_ERROR", "Invalid current password."));
                }

                existingUser.PasswordHash = _authService.HashPassword(entity.Password);
            }

            await _repository.UpdateAsync(existingUser);
            await _repository.SaveChangesAsync();
            return NoContent();
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
                var count = (await _repository.GetAllAsync()).Count();
                entity.ClientCode = $"CLT-{(count + 1):D3}";
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
    }
}
