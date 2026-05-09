using Microsoft.AspNetCore.Mvc;
using AATS.Application.Common.Interfaces;
using AATS.API.Models;

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
            var result = new PaginatedResult<T>
            {
                Items = list,
                TotalCount = list.Count
            };
            return Ok(ApiResponse<PaginatedResult<T>>.Ok(result));
        }

        [HttpGet("{id}")]
        public virtual async Task<ActionResult<ApiResponse<T>>> GetById(Guid id)
        {
            var item = await _repository.GetByIdAsync(id);
            if (item == null) return NotFound(ApiResponse<T>.Failure("NOT_FOUND", "Item not found"));
            return Ok(ApiResponse<T>.Ok(item));
        }

        [HttpPost]
        public virtual async Task<ActionResult<ApiResponse<T>>> Create(T entity)
        {
            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();
            var response = ApiResponse<T>.Ok(entity);
            return CreatedAtAction(nameof(GetById), new { id = (entity as dynamic).Id }, response);
        }

        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Update(Guid id, T entity)
        {
            if (id != (entity as dynamic).Id) return BadRequest();
            await _repository.UpdateAsync(entity);
            await _repository.SaveChangesAsync();
            return NoContent();
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
