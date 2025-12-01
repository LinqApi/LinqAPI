using LinqApi.Core;
using LinqApi.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Collections;

namespace LinqApi.Controller;

[Route("api/[controller]")]
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

    public static class ViewModelSchemaHelper
    {
        private const string displayPropertyName = "displayProperty";
        private static readonly ConcurrentDictionary<string, List<object>> _schemaCache = new();

        public static List<object> GetSchema (Type type)
        {
            var cacheKey = $"Schema_{type.FullName}";
            if (_schemaCache.TryGetValue(cacheKey, out var cached))
                return cached;

            // parametresiz ctor yoksa null kalabilir, default değerler için önemli değilse sorun değil
            object? instance = null;
            try
            {
                instance = Activator.CreateInstance(type);
            }
            catch
            {
                // yutuyoruz; default değerleri okuyamazsak @default = null gider
            }

            var props = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select<PropertyInfo, object>(p =>
                {
                    var rawType = p.PropertyType;
                    var actualType = Nullable.GetUnderlyingType(rawType) ?? rawType;

                    // EF dışı tipleri erken safhada yakala (geography, hierarchyid, byte[])
                    EnsureTypeIsSupportedForEf(p, rawType, actualType);

                    var rawName = ToCamelCase(p.Name);

                    // Nullable bilgisi:
                    // - Nullable<T> → true
                    // - reference type → true
                    // - value type → false
                    var isNullable =
                        Nullable.GetUnderlyingType(rawType) != null ||
                        (!actualType.IsValueType && rawType != typeof(string) /* string optional kabul edilebilir */) ||
                        rawType == typeof(string); // string'i de nullable sayalım

                    // Koleksiyon tiplerini (complexList) tespit
                    var isCollection =
                        typeof(IEnumerable).IsAssignableFrom(rawType)
                        && rawType != typeof(string)
                        && rawType != typeof(byte[]); // byte[]'i collection gibi görme

                    if (isCollection)
                    {
                        // ICollection<SpotifyAlbum> -> SpotifyAlbum
                        var baseType = actualType.IsGenericType
                            ? actualType.GetGenericArguments().FirstOrDefault()
                            : null;

                        var collectionInfo = new
                        {
                            name = rawName,
                            kind = "complexList", // <-- mevcut FE kontratı
                            baseType = baseType?.Name,
                            type = GetReadableTypeName(rawType),
                            // yeni alanlar (eskisini bozmadan ekliyoruz)
                            isNullable = false, // koleksiyonlar için anlamlı değil, false diyelim
                        };

                        return (object)collectionInfo;
                    }

                    // Simple / enum / complex ayrımı
                    var isEnum = actualType.IsEnum;
                    var isSimpleType = IsSupportedSimpleType(actualType);

                    if (!isSimpleType && !isEnum)
                    {
                        // Tekil complex (navigation) tipler: MediaAsset, Location, başka entity’ler
                        var complexInfo = new
                        {
                            name = rawName,
                            kind = "complex",
                            baseType = actualType.Name,
                            type = GetReadableTypeName(rawType),
                            isNullable,
                            // complex için display/validation yok; FE isterse genişletirsin
                        };

                        return (object)complexInfo;
                    }

                    // --- Basit (string, int, bool, DateTime, Guid) + enum tipler ---

                    var displayAttr = p.GetCustomAttribute<DisplayAttribute>();
                    var descAttr = p.GetCustomAttribute<DescriptionAttribute>();

                    var display = new Dictionary<string, object?>();

                    display["name"] = displayAttr?.Name ?? rawName;

                    if (!string.IsNullOrWhiteSpace(descAttr?.Description))
                        display["description"] = descAttr.Description;

                    if (isEnum)
                    {
                        var values = Enum.GetValues(actualType)
                            .Cast<object>()
                            .Select(ev =>
                            {
                                var memberName = ev.ToString() ?? string.Empty;
                                var memInfo = actualType.GetMember(memberName).FirstOrDefault();
                                var evDisplay = memInfo?.GetCustomAttribute<DisplayAttribute>();
                                var evDesc = memInfo?.GetCustomAttribute<DescriptionAttribute>();

                                return new
                                {
                                    name = memberName,
                                    value = Convert.ToInt32(ev),
                                    displayName = evDisplay?.Name,
                                    description = evDesc?.Description
                                };
                            })
                            .ToList();

                        display["values"] = values;
                    }

                    var isRequired = p.GetCustomAttributes()
                        .Any(a => a.GetType().Name.Contains("Required", StringComparison.OrdinalIgnoreCase));

                    // EF convention: Id long ise PK say
                    var isPrimaryKey =
                        p.GetCustomAttribute<KeyAttribute>() != null ||
                        (string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase)
                         && (actualType == typeof(long) || actualType == typeof(long?)));

                    var validation = BuildValidation(p);

                    var simpleInfo = new
                    {
                        name = rawName,
                        kind = isEnum ? "enum" : "simple",
                        type = GetReadableTypeName(p.PropertyType),
                        // eski alanlar
                        @default = instance != null ? p.GetValue(instance) : null,
                        isRequired,
                        isPrimaryKey,
                        display,
                        // yeni alanlar
                        isNullable,
                        validation
                    };

                    return (object)simpleInfo;
                })
                .Cast<object>()
                .ToList();

            var displayProp = type.GetCustomAttribute<DisplayPropertyAttribute>();
            if (displayProp != null)
            {
                // Eski davranışı koruyoruz: listeye ekstra bir obje ekleniyor
                props.Add(new
                {
                    type = displayPropertyName,
                    properties = displayProp.Properties
                });
            }

            _schemaCache.TryAdd(cacheKey, props);
            return props;
        }

        private static void EnsureTypeIsSupportedForEf (PropertyInfo property, Type rawType, Type actualType)
        {
            // byte[] direkt yasak
            if (rawType == typeof(byte[]) || actualType == typeof(byte[]))
            {
                throw new NotSupportedException(
                    $"Property '{property.Name}' on '{property.DeclaringType?.Name}' is of type byte[], " +
                    "which is not allowed in LINQ API schema. Use MediaAsset or a separate storage mechanism instead.");
            }

            // geography / hierarchyid / SQL Server spatial
            var ns = actualType.Namespace ?? string.Empty;
            var tn = actualType.Name;

            if (ns.StartsWith("Microsoft.SqlServer.Types", StringComparison.OrdinalIgnoreCase) &&
                (tn.Contains("Geography", StringComparison.OrdinalIgnoreCase) ||
                 tn.Contains("HierarchyId", StringComparison.OrdinalIgnoreCase)))
            {
                throw new NotSupportedException(
                    $"Property '{property.Name}' on '{property.DeclaringType?.Name}' is of type '{actualType.FullName}', " +
                    "which is not supported. Spatial / hierarchy types are not allowed in LINQ API entities.");
            }

            // "geography"/"geometry" benzeri custom wrapper tipler kullanılırsa yine block’lamak istersen:
            if (tn.Equals("Geography", StringComparison.OrdinalIgnoreCase) ||
                tn.Equals("Geometry", StringComparison.OrdinalIgnoreCase))
            {
                throw new NotSupportedException(
                    $"Property '{property.Name}' on '{property.DeclaringType?.Name}' is of type '{actualType.FullName}', " +
                    "which is not supported. Use your Location model or EF-supported scalar types instead.");
            }
        }

        private static bool IsSupportedSimpleType (Type actualType)
        {
            if (actualType.IsEnum)
                return true;

            // EF Core'ın standart scalar tipleri (bilerek kısıtlı tutuyoruz)
            if (actualType == typeof(string) ||
                actualType == typeof(bool) ||
                actualType == typeof(byte) ||
                actualType == typeof(short) ||
                actualType == typeof(int) ||
                actualType == typeof(long) ||
                actualType == typeof(float) ||
                actualType == typeof(double) ||
                actualType == typeof(decimal) ||
                actualType == typeof(DateTime) ||
                actualType == typeof(DateTimeOffset) ||
                actualType == typeof(TimeSpan) ||
                actualType == typeof(Guid))
            {
                return true;
            }

            // Diğer primitive’ler vs. (sbyte, ushort, uint, ulong) → desteklemek istemiyorsan false bırak
            if (actualType.IsPrimitive)
                return false;

            return false;
        }

        private static Dictionary<string, object?>? BuildValidation (PropertyInfo p)
        {
            var dict = new Dictionary<string, object?>();

            var stringLength = p.GetCustomAttribute<StringLengthAttribute>();
            var maxLength = p.GetCustomAttribute<MaxLengthAttribute>();
            var minLength = p.GetCustomAttribute<MinLengthAttribute>();
            var range = p.GetCustomAttribute<RangeAttribute>();
            var regex = p.GetCustomAttribute<RegularExpressionAttribute>();

            var required = p.GetCustomAttributes()
                .Any(a => a.GetType().Name.Contains("Required", StringComparison.OrdinalIgnoreCase));
            dict["isRequired"] = required;

            if (stringLength != null)
            {
                dict["maxLength"] = stringLength.MaximumLength;
                if (stringLength.MinimumLength > 0)
                    dict["minLength"] = stringLength.MinimumLength;
            }

            if (maxLength != null)
            {
                dict["maxLength"] = maxLength.Length;
            }

            if (minLength != null)
            {
                dict["minLength"] = minLength.Length;
            }

            if (range != null)
            {
                dict["min"] = range.Minimum;
                dict["max"] = range.Maximum;
            }

            if (regex != null)
            {
                dict["pattern"] = regex.Pattern;
            }

            // Eğer sadece isRequired varsa bile dönmek ister misin?
            // Evet, FE tarafında tek yerden okumak için güzel.
            return dict.Count > 0 ? dict : null;
        }

        private static string ToCamelCase (string input)
        {
            if (string.IsNullOrEmpty(input) || char.IsLower(input[0]))
                return input;

            return char.ToLowerInvariant(input[0]) + input.Substring(1);
        }

        public static string GetReadableTypeName (Type type)
        {
            if (type.IsGenericType)
            {
                var genericTypeDefinition = type.GetGenericTypeDefinition();
                var genericArgs = type.GetGenericArguments();

                var typeName = genericTypeDefinition.Name;
                var backtickIndex = typeName.IndexOf('`');
                if (backtickIndex > 0)
                {
                    typeName = typeName[..backtickIndex];
                }

                var genericArgNames = string.Join(", ", genericArgs.Select(GetReadableTypeName));
                return $"{typeName}<{genericArgNames}>";
            }

            return type.Name;
        }
    }