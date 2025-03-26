using LinqApi.Model;

namespace LinqApi.Controller
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using System;
    using LinqApi.Repository;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Reflection;

    /// <summary>
    /// LINQ controller that supports Create and Update operations via ViewModels (TCreateVm, TUpdateVm).
    /// Maps incoming DTOs to the entity model, useful for separating concerns and enforcing boundaries.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public abstract class LinqVmController<TEntity, TCreateVm, TUpdateVm, TId>(ILinqRepository<TEntity, TId> repo) : LinqReadonlyController<TEntity, TId>(repo)
        where TEntity : BaseEntity<TId>
    {
        private static readonly ConcurrentDictionary<string, List<object>> _vmSchemaCache = new();

        [HttpGet("create-vm-schema")]
        public virtual IActionResult GetCreateVmSchema() => Ok(GetVmSchema(typeof(TCreateVm)));

        [HttpGet("update-vm-schema")]
        public virtual IActionResult GetUpdateVmSchema() => Ok(GetVmSchema(typeof(TUpdateVm)));

        [HttpPost]
        public virtual async Task<IActionResult> Create([FromBody] TCreateVm vm, CancellationToken cancellation)
        {
            var entity = MapToEntityFromCreate(vm);
            var created = await _repo.InsertAsync(entity, cancellation);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        
        
        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Update([FromRoute] TId id, [FromBody] TUpdateVm vm, CancellationToken cancellationToken)
        {
            if (id == null)
            {
                return BadRequest("Id is required.");
            }

            try
            {
                // Map the update view model to the entity.
                var entity = MapToEntityFromUpdate(vm);

                // Optionally, you might want to check if the entity exists here before updating.
                var updated = await _repo.UpdateAsync(entity, cancellationToken);
                if (updated == null)
                {
                    return NotFound($"Entity with id {id} was not found.");
                }

                return Ok(updated);
            }
            catch (Exception ex)
            {
                // Log the exception as needed.
                return StatusCode(500, new { message = "An error occurred during update.", error = ex.Message });
            }
        }


        private List<object> GetVmSchema(Type vmType)
        {
            var cacheKey = $"VmSchema_{vmType.FullName}";
            if (_vmSchemaCache.TryGetValue(cacheKey, out var cached)) return cached;

            var instance = Activator.CreateInstance(vmType);
            var props = vmType.GetProperties()
                .Select(p => new
                {
                    Name = p.Name,
                    Type = GetReadableTypeName(p.PropertyType),
                    Default = p.GetValue(instance),
                    IsRequired = p.GetCustomAttributes().Any(a => a.GetType().Name.Contains("Required"))
                })
                .Cast<object>()
                .ToList();

            _vmSchemaCache.TryAdd(cacheKey, props);
            return props;
        }

        protected abstract TEntity MapToEntityFromCreate(TCreateVm vm);
        protected abstract TEntity MapToEntityFromUpdate(TUpdateVm vm);
    }
}


