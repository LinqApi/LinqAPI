
namespace LinqApi.Controller
{
    using LinqApi.Logging;
    using LinqApi.Repository;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Data.SqlTypes;
    using System.Reflection;
    using System.Threading.Tasks;

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
        private const string displayPropertyName = "displayProperty";
        private static readonly ConcurrentDictionary<string, List<object>> _schemaCache = new();

        public static List<object> GetSchema(Type type)
        {
            var cacheKey = $"Schema_{type.FullName}";
            if (_schemaCache.TryGetValue(cacheKey, out var cached))
                return cached;

            var instance = Activator.CreateInstance(type);

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p =>
                {
                    var rawType = p.PropertyType;
                    var isNullable = Nullable.GetUnderlyingType(rawType) != null;
                    var actualType = isNullable
                        ? Nullable.GetUnderlyingType(rawType)!
                        : rawType;

                    var isEnum = actualType.IsEnum;
                    var rawName = ToCamelCase(p.Name);

                    var displayAttr = p.GetCustomAttribute<DisplayAttribute>();
                    var descAttr = p.GetCustomAttribute<DescriptionAttribute>();

                    var display = new Dictionary<string, object?>
                    {
                        ["name"] = displayAttr?.Name ?? rawName
                    };

                    if (!string.IsNullOrWhiteSpace(descAttr?.Description))
                        display["description"] = descAttr.Description;

                    // Eğer enum ise, values dizisini ekle
                    if (isEnum)
                    {
                        var values = Enum.GetValues(actualType)
                            .Cast<object>()
                            .Select(ev =>
                            {
                                var memInfo = actualType.GetMember(ev.ToString() ?? "").FirstOrDefault();
                                var evDisplay = memInfo?.GetCustomAttribute<DisplayAttribute>();
                                var evDesc = memInfo?.GetCustomAttribute<DescriptionAttribute>();

                                return new
                                {
                                    name = ev.ToString(),
                                    value = Convert.ToInt32(ev),
                                    displayName = evDisplay?.Name,
                                    description = evDesc?.Description
                                };
                            })
                            .ToList();

                        display["values"] = values;
                    }

                    return new
                    {
                        name = rawName,
                        type = isEnum ? "Enum" : GetReadableTypeName(p.PropertyType),
                        @default = p.GetValue(instance),
                        isRequired = p.GetCustomAttributes().Any(a => a.GetType().Name.Contains("Required")),
                        display
                    };
                })
                .Cast<object>()
                .ToList();

            var displayProp = type.GetCustomAttribute<DisplayPropertyAttribute>();
            if (displayProp != null)
            {
                props.Add(new
                {
                    type = displayPropertyName,
                    properties = displayProp.Properties
                });
            }

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