using LinqApi.Model;
using System.Linq.Dynamic.Core;

namespace LinqApi.Controller
{
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System;
    using LinqApi.Repository;

    [ApiController]
    [Route("api/[controller]")]  // Bunu değiştiriyoruz!
    public class LinqController<TEntity, TId> : ControllerBase
    where TEntity : BaseEntity<TId>
    {
        private readonly ILinqRepository<TEntity, TId> _repo;
        private static readonly ConcurrentDictionary<string, Expression<Func<TEntity, bool>>> _filterCache = new();
        private static readonly ConcurrentDictionary<string, List<object>> _propertyCache = new();

        public LinqController(ILinqRepository<TEntity, TId> repo)
        {
            _repo = repo;
        }

        [HttpGet("properties")]
        public IActionResult GetAllProperties()
        {
            var type = typeof(TEntity);
            var cacheKey = $"Properties_{type.FullName}";

            if (_propertyCache.TryGetValue(cacheKey, out var cachedProperties))
            {
                return Ok(cachedProperties);
            }

            var properties = type.GetProperties()
                .Select(p => new { Name = p.Name, Type = GetReadableTypeName(p.PropertyType) })
                .Cast<object>()
                .ToList();

            _propertyCache.TryAdd(cacheKey, properties);
            return Ok(properties);
        }

        private string GetReadableTypeName(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return $"{Nullable.GetUnderlyingType(type)?.Name}?";

            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments().Select(GetReadableTypeName);
                return $"{type.Name.Split('`')[0]}<{string.Join(", ", genericArguments)}>";
            }
            return type.Name;
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(TId id)
        {
            var dto = await _repo.GetByIdAsync(id);
            if (dto == null)
                return NotFound();

            return Ok(dto);
        }

        /// <summary>
        /// Yeni bir DTO ekler.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] TEntity entity)
        {
            var createdDto = await _repo.InsertAsync(entity);
            return CreatedAtAction(nameof(GetById), new { id = createdDto.Id }, createdDto);
        }


        [HttpPut]
        public async Task<IActionResult> Update([FromBody] TEntity entity)
        {
            var updatedDto = await _repo.UpdateAsync(entity);
            return Ok(updatedDto);
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(TId id)
        {
            await _repo.DeleteAsync(id);
            return NoContent();
        }


        [HttpPost("filterpaged")]
        public async Task<IActionResult> GetByFilterPaged([FromBody] LinqFilterModel filterPageModel)
        {
            if (string.IsNullOrWhiteSpace(filterPageModel.Filter))
                return BadRequest("Invalid filter expression.");

            // Tüm dinamik sorgu seçeneklerini işleyen repository metodunu çağırıyoruz.
            var pagedResult = await _repo.GetFilterPagedAsync(filterPageModel);
            return Ok(pagedResult);
        }

    }
}