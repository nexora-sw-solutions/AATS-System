using System;
using System.Linq;
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
        public virtual async Task<ActionResult<ApiResponse<PaginatedResult<T>>>> GetAll()
        {
            var items = await _repository.GetAllAsync();
            var list = items.ToList();
            Console.WriteLine($"[DEBUG] GetAll for {typeof(T).Name}. Found {list.Count} items.");
            
            var result = new PaginatedResult<T>
            {
                Items = list,
                TotalCount = list.Count
            };

            if (list.Any() && list.First() is RecordBase)
            {
                var recordService = HttpContext.RequestServices.GetService(typeof(IRecordService)) as IRecordService;
                if (recordService != null)
                {
                    await recordService.EnrichRecordsAsync(list.Cast<RecordBase>());
                }
            }

            return Ok(ApiResponse<PaginatedResult<T>>.Ok(result));
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<ApiResponse<T>>> GetById(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<T>.Failure("NOT_FOUND", "Item not found"));

            if (item is RecordBase recordBase)
            {
                var recordService = HttpContext.RequestServices.GetService(typeof(IRecordService)) as IRecordService;
                if (recordService != null)
                {
                    await recordService.EnrichRecordsAsync(new[] { recordBase });
                }
            }

            return Ok(ApiResponse<T>.Ok(item));
        }

        [HttpPost]
        public virtual async Task<ActionResult<ApiResponse<T>>> Create(T entity)
        {
            try
            {
                if (entity is RecordBase rb)
                {
                    var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                    if (Guid.TryParse(userIdClaim, out Guid userId))
                    {
                        rb.CreatedBy = userId;
                    }

                    var recordService = HttpContext.RequestServices.GetService(typeof(IRecordService)) as IRecordService;
                    if (recordService != null)
                    {
                        await recordService.ProcessChequeDetailsAsync(rb, typeof(T).Name);
                    }
                }

                await _repository.AddAsync(entity);
                await _repository.SaveChangesAsync();
                return CreatedAtAction(nameof(GetById), new { id = (entity as dynamic).Id }, ApiResponse<T>.Ok(entity));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<T>.Failure("SERVER_ERROR", ex.Message));
            }
        }

        [HttpPut("{id}")]
        public virtual async Task<ActionResult<ApiResponse<T>>> Update(Guid id, T entity)
        {
            try
            {
                var existing = await _repository.GetByIdAsync(id);
                if (existing == null) return NotFound(ApiResponse<T>.Failure("NOT_FOUND", "Item not found"));

                if (entity is RecordBase rb && existing is RecordBase existingRb)
                {
                    // Preserve immutable/server-managed fields if not provided by frontend
                    if (string.IsNullOrEmpty(rb.RecordCode))
                    {
                        rb.RecordCode = existingRb.RecordCode;
                    }
                    
                    if (rb.CreatedBy == null || rb.CreatedBy == Guid.Empty)
                    {
                        rb.CreatedBy = existingRb.CreatedBy;
                    }

                    var recordService = HttpContext.RequestServices.GetService(typeof(IRecordService)) as IRecordService;
                    if (recordService != null)
                    {
                        await recordService.ProcessChequeDetailsAsync(rb, typeof(T).Name);
                    }
                }

                // Use SetValues to update the existing tracked entity from the new detached entity
                // This avoids tracking conflicts where an entity with the same ID is already loaded
                var context = HttpContext.RequestServices.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
                if (context != null)
                {
                    context.Entry(existing).CurrentValues.SetValues(entity);
                }
                else
                {
                    await _repository.UpdateAsync(entity);
                }
                
                await _repository.SaveChangesAsync();
                return Ok(ApiResponse<T>.Ok(existing));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<T>.Failure("SERVER_ERROR", ex.Message));
            }
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound();
            await _repository.DeleteAsync(item);
            await _repository.SaveChangesAsync();
            return NoContent();
        }
    }
}
