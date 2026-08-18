using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AATS.Application.Common.Interfaces;
using AATS.API.Models;
using AATS.Domain.Entities;
using AATS.Infrastructure.Persistence;

namespace AATS.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public abstract class BaseApiController<T> : ControllerBase where T : class
    {
        protected readonly IRepository<T> _repository;

        protected BaseApiController(IRepository<T> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public virtual async Task<ActionResult<ApiResponse<PaginatedResult<T>>>> GetAll([FromQuery] bool enrich = true, [FromQuery] bool includeDeleted = true)
        {
            var list = await _repository.GetAllAsync();
            var now = DateTime.UtcNow;
            var toPurge = new List<T>();
            var validItems = new List<T>();

            foreach (var item in list)
            {
                var isDelProp = item.GetType().GetProperty("IsDeleted");
                var delAtProp = item.GetType().GetProperty("DeletedAt");
                if (isDelProp != null && (bool)(isDelProp.GetValue(item) ?? false))
                {
                    var deletedAt = delAtProp?.GetValue(item) as DateTime?;
                    if (deletedAt.HasValue && (now - deletedAt.Value).TotalDays >= 30)
                    {
                        toPurge.Add(item);
                    }
                    else if (includeDeleted)
                    {
                        validItems.Add(item);
                    }
                }
                else
                {
                    validItems.Add(item);
                }
            }

            if (toPurge.Any())
            {
                foreach (var p in toPurge)
                {
                    await _repository.DeleteAsync(p);
                }
                await _repository.SaveChangesAsync();
            }

            var filtered = await FilterListForCurrentUserAsync(validItems);
            var result = new PaginatedResult<T>
            {
                Items = filtered,
                TotalCount = filtered.Count
            };
            return Ok(ApiResponse<PaginatedResult<T>>.Ok(result));
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<ApiResponse<T>>> GetById(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null || !await CheckUserAccessAsync(item))
            {
                return NotFound(ApiResponse<T>.Failure("NOT_FOUND", "Item not found"));
            }
            return Ok(ApiResponse<T>.Ok(item));
        }

        [HttpPost]
        public virtual async Task<ActionResult<ApiResponse<T>>> Create([FromBody] T entity)
        {
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
            return Ok(ApiResponse<T>.Ok(entity));
        }

        [HttpPut("{id}")]
        public virtual async Task<ActionResult<ApiResponse<T>>> Update(Guid id, [FromBody] T entity)
        {
            await _repository.UpdateAsync(entity);
            await _repository.SaveChangesAsync();
            return Ok(ApiResponse<T>.Ok(entity));
        }

        [HttpDelete("{id}")]
        public virtual async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item != null)
            {
                var isDelProp = item.GetType().GetProperty("IsDeleted");
                var delAtProp = item.GetType().GetProperty("DeletedAt");
                var statusProp = item.GetType().GetProperty("Status");

                if (isDelProp != null && delAtProp != null)
                {
                    isDelProp.SetValue(item, true);
                    delAtProp.SetValue(item, DateTime.UtcNow);
                    if (statusProp != null && statusProp.CanWrite)
                    {
                        statusProp.SetValue(item, "Inactive");
                    }
                    await _repository.UpdateAsync(item);
                }
                else
                {
                    await _repository.DeleteAsync(item);
                }
                await _repository.SaveChangesAsync();
            }
            return Ok(ApiResponse<bool>.Ok(true));
        }

        [HttpGet("deleted")]
        public virtual async Task<ActionResult<ApiResponse<PaginatedResult<T>>>> GetDeleted()
        {
            var list = await _repository.GetAllAsync();
            var deletedItems = new List<T>();
            var now = DateTime.UtcNow;

            foreach (var item in list)
            {
                var isDelProp = item.GetType().GetProperty("IsDeleted");
                var delAtProp = item.GetType().GetProperty("DeletedAt");
                if (isDelProp != null && (bool)(isDelProp.GetValue(item) ?? false))
                {
                    var deletedAt = delAtProp?.GetValue(item) as DateTime?;
                    if (deletedAt.HasValue && (now - deletedAt.Value).TotalDays >= 30)
                    {
                        await _repository.DeleteAsync(item);
                    }
                    else
                    {
                        deletedItems.Add(item);
                    }
                }
            }
            await _repository.SaveChangesAsync();

            var filtered = await FilterListForCurrentUserAsync(deletedItems);
            var result = new PaginatedResult<T>
            {
                Items = filtered,
                TotalCount = filtered.Count
            };
            return Ok(ApiResponse<PaginatedResult<T>>.Ok(result));
        }

        [HttpPost("{id}/restore")]
        public virtual async Task<ActionResult<ApiResponse<bool>>> Restore(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item != null)
            {
                var isDelProp = item.GetType().GetProperty("IsDeleted");
                var delAtProp = item.GetType().GetProperty("DeletedAt");
                var statusProp = item.GetType().GetProperty("Status");

                if (isDelProp != null && delAtProp != null)
                {
                    isDelProp.SetValue(item, false);
                    delAtProp.SetValue(item, null);
                    if (statusProp != null && statusProp.CanWrite)
                    {
                        statusProp.SetValue(item, "Active");
                    }
                    await _repository.UpdateAsync(item);
                    await _repository.SaveChangesAsync();
                }
            }
            return Ok(ApiResponse<bool>.Ok(true));
        }

        [HttpDelete("{id}/permanent")]
        public virtual async Task<ActionResult<ApiResponse<bool>>> PermanentDelete(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item != null)
            {
                await _repository.DeleteAsync(item);
                await _repository.SaveChangesAsync();
            }
            return Ok(ApiResponse<bool>.Ok(true));
        }

        protected async Task<(bool IsStaff, Guid? BranchId)> GetCurrentUserRoleAndBranchAsync()
        {
            var context = HttpContext.RequestServices.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
            if (context == null) return (false, null);

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var userId))
            {
                var userObj = await context.Users.FindAsync(userId);
                if (userObj != null && userObj.Role == UserRole.Staff)
                {
                    return (true, userObj.BranchId);
                }
            }
            return (false, null);
        }

        protected async Task<bool> CheckUserAccessAsync(object? item)
        {
            if (item == null) return false;
            var (isStaff, branchId) = await GetCurrentUserRoleAndBranchAsync();
            if (!isStaff || !branchId.HasValue) return true;

            var prop = item.GetType().GetProperty("BranchId");
            if (prop != null)
            {
                var val = prop.GetValue(item) as Guid?;
                return val == null || val == branchId;
            }
            return true;
        }

        protected async Task<List<T>> FilterListForCurrentUserAsync(IEnumerable<T> items)
        {
            var (isStaff, branchId) = await GetCurrentUserRoleAndBranchAsync();
            if (!isStaff || !branchId.HasValue) return items.ToList();

            var filtered = new List<T>();
            foreach (var item in items)
            {
                if (await CheckUserAccessAsync(item))
                {
                    filtered.Add(item);
                }
            }
            return filtered;
        }
    }
}
