using LinqApi.Model;
using LinqApi.Service;
using System.Linq.Dynamic.Core;

namespace LinqApi.Controller
{
    using System.Collections.Concurrent;
    using System.Linq.Expressions;
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;

    /// <summary>
    /// Dinamik LINQ filtreleme ve CRUD işlemleri sağlayan API Controller.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class LinqController<T1, T2, TId> : ControllerBase
        where T1 : class
        where T2 : BaseDto<TId>
    {
        private readonly ILinqService<T1, T2, TId> _service;
        private static readonly ConcurrentDictionary<string, Expression<Func<T1, bool>>> _filterCache = new();
        private static readonly ConcurrentDictionary<string, List<object>> _propertyCache = new();

        public LinqController(ILinqService<T1, T2, TId> service)
        {
            _service = service;
        }

        /// <summary>
        /// Dönüştürülebilir T2 tipi için property listesini getirir.
        /// </summary>
        [HttpGet("properties")]
        public IActionResult GetAllProperties()
        {
            var type = typeof(T2);
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

        /// <summary>
        /// Belirtilen ID’ye sahip DTO’yu getirir.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(TId id)
        {
            var dto = await _service.GetByIdAsync(id);
            if (dto == null)
                return NotFound();

            return Ok(dto);
        }

        /// <summary>
        /// Yeni bir DTO ekler.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] T2 dto)
        {
            var createdDto = await _service.InsertAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = createdDto.Id }, createdDto);
        }

        /// <summary>
        /// Var olan bir DTO’yu günceller.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(TId id, [FromBody] T2 dto)
        {
            if (!id.Equals(dto.Id))
                return BadRequest("ID mismatch.");

            var updatedDto = await _service.UpdateAsync(dto);
            return Ok(updatedDto);
        }

        /// <summary>
        /// Belirtilen ID’ye sahip DTO’yu siler.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(TId id)
        {
            await _service.DeleteAsync(id);
            return NoContent();
        }

        /// <summary>
        /// Dinamik LINQ filtre ifadesine göre DTO’ları getirir.
        /// </summary>
        [HttpPost("filter")]
        public async Task<IActionResult> GetByFilter([FromBody] LinqFilterModel filterModel)
        {
            if (string.IsNullOrWhiteSpace(filterModel.Filter))
                return BadRequest("Invalid filter expression.");

            if (!_filterCache.TryGetValue(filterModel.Filter, out var predicate))
            {
                predicate = DynamicExpressionParser.ParseLambda<T1, bool>(null, false, filterModel.Filter);
                _filterCache.TryAdd(filterModel.Filter, predicate);
            }

            var dtos = await _service.FindAsync(predicate);
            return Ok(dtos);
        }

        /// <summary>
        /// Dinamik LINQ filtre ifadesi ve sıralama ile sayfalı sonuç getirir.
        /// </summary>
        [HttpPost("filterpaged")]
        public async Task<IActionResult> GetByFilterPaged([FromBody] LinqFilterModel filterPageModel)
        {
            if (string.IsNullOrWhiteSpace(filterPageModel.Filter))
                return BadRequest("Invalid filter expression.");

            if (!_filterCache.TryGetValue(filterPageModel.Filter, out var predicate))
            {
                predicate = DynamicExpressionParser.ParseLambda<T1, bool>(null, false, filterPageModel.Filter);
                _filterCache.TryAdd(filterPageModel.Filter, predicate);
            }

            Expression<Func<T1, object>> orderBy = null;
            if (!string.IsNullOrWhiteSpace(filterPageModel.Orderby))
            {
                orderBy = (Expression<Func<T1, object>>)DynamicExpressionParser.ParseLambda<T1, object>(null, false, filterPageModel.Orderby);
            }

            var pagedResult = await _service.GetPagedFilteredAsync(
                predicate,
                filterPageModel.Pager.PageNumber,
                filterPageModel.Pager.PageSize,
                filterPageModel.Includes,
                orderBy,
                filterPageModel.Desc
            );

            return Ok(pagedResult);
        }
    }
}