
namespace LinqApi.Controller
{
    using Microsoft.AspNetCore.Mvc;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using System;
    using LinqApi.Repository;
    using System.Reflection;
    using LinqApi.Core;
    using System.Collections.Concurrent;
    using Microsoft.AspNetCore.Routing;

    [ApiController]
    public class LinqController<TEntity, TId>(ILinqRepository<TEntity, TId> repo) : LinqReadonlyController<TEntity, TId>(repo)
     where TEntity : BaseEntity<TId>
    {

        [HttpPost]
        public virtual async Task<IActionResult> Create([FromBody] TEntity entity, CancellationToken cancellationToken)
        {
            var createdDto = await _repo.InsertAsync(entity, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = createdDto.Id }, createdDto);
        }

        [HttpPut("{id}")]
        public virtual async Task<IActionResult> Update(TId id, [FromBody] TEntity entity, CancellationToken cancellationToken)
        {
            entity.Id = id;
            var updatedDto = await _repo.UpdateAsync(entity, cancellationToken);
            return Ok(updatedDto);
        }

        [HttpDelete("{id}")]
        public virtual async Task<IActionResult> Delete(TId id, CancellationToken cancellationToken)
        {
            await _repo.DeleteAsync(id, cancellationToken);
            return NoContent();
        }





    }
    /// <summary>
    /// Provides helper methods to generate a property schema for entity types.
    /// </summary>
    public static class ViewModelSchemaHelper
    {
        private static readonly ConcurrentDictionary<string, List<object>> _schemaCache = new();

        public static List<object> GetSchema(Type type)
        {
            var cacheKey = $"Schema_{type.FullName}";
            if (_schemaCache.TryGetValue(cacheKey, out var cached))
                return cached;

            var instance = Activator.CreateInstance(type);
            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new
                {
                    Name = ToCamelCase(p.Name),
                    Type = GetReadableTypeName(p.PropertyType),
                    Default = p.GetValue(instance),
                    IsRequired = p.GetCustomAttributes().Any(a => a.GetType().Name.Contains("Required"))
                })
                .Cast<object>()
                .ToList();

            _schemaCache.TryAdd(cacheKey, props);
            return props;
        }

        private static string ToCamelCase(string input)
        {
            if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
                return input;

            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        }


        public static string GetReadableTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var genericArgs = type.GetGenericArguments();
                // Generic tipin adını alıp "`1" kısmını kaldırıyoruz.
                var typeName = genericTypeDefinition.Name;
                var backtickIndex = typeName.IndexOf('`');
                if (backtickIndex > 0)
                {
                    typeName = typeName.Substring(0, backtickIndex);
                }
                // Generic argümanları da okunabilir hale getiriyoruz.
                var genericArgNames = string.Join(", ", genericArgs.Select(t => GetReadableTypeName(t)));
                return $"{typeName}<{genericArgNames}>";
            }
            return type.Name;
        }
    }
}