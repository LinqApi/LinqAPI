using LinqApi.Dynamic.Assembly;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Reflection.Emit;
using System.Reflection;
using LinqApi.Controller;
using LinqApi.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LinqApi.Dynamic.Controller
{
    public class LinqApiApmOptions
    {
        /// <summary>
        /// Üretilecek controller'ların DbContext tipini belirtir.
        /// </summary>
        public Type DbContextType { get; set; }

        /// <summary>
        /// İsteğe göre, entity tiplerini filtrelemek için kullanılabilir.
        /// Örneğin, bazı entity'ler için dinamik controller üretilmesin istenebilir.
        /// Default'ta true döner, yani tüm entity'ler geçerli.
        /// </summary>
        public Func<Type, bool> EntityFilter { get; set; } = type => true;

        /// <summary>
        /// Hangi LinqController tiplerinin üretileceğini tutan bir liste/dizi.
        /// Örneğin: new[] { LinqControllerType.LinqReadonlyController, LinqControllerType.LinqController }
        /// </summary>
        public LinqControllerType ControllerType { get; set; } = LinqControllerType.LinqController;

        /// <summary>
        /// Controller'ların yer alacağı "Area" ismi.
        /// </summary>
        public string AreaName { get; set; } = string.Empty;
    }


    public enum LinqControllerType
    {
        LinqReadonlyController = 0,
        LinqController = 10,
        LinqVmController = 20,
        LinqLogController = 30,
        LinqLocalizationController = 40
    }
    public static class LinqApiRegistry
    {
        private static readonly List<DynamicApiInfo> _apis = new List<DynamicApiInfo>();

        public static IReadOnlyList<DynamicApiInfo> Apis => _apis;

        public static void Register(DynamicApiInfo info)
        {
            _apis.Add(info);
        }

        public static void RegisterRange(IEnumerable<DynamicApiInfo> infos)
        {
            _apis.AddRange(infos);
        }
    }

    public class DynamicApiInfo
    {
        public string EntityName { get; set; }
        public string ControllerName { get; set; }
        public string RoutePrefix { get; set; } // ex: "api/[area]/[controller]"
        public string AreaName { get; set; }    // ex: "DynamicArea"
    }
    public class LinqApiControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly LinqApiApmOptions _options;

        public LinqApiControllerFeatureProvider(LinqApiApmOptions options)
        {
            _options = options;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            // 1) DbContext içindeki DbSet<> property'lerini bul.
            var entityTypes = _options.DbContextType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                    p.PropertyType.IsGenericType &&
                    p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                )
                .Select(p => p.PropertyType.GetGenericArguments()[0])
                // BaseEntity<TId> türevi ama *somut* mu (IsConcreteSubclassOfGenericBaseEntity)
                .Where(IsConcreteSubclassOfGenericBaseEntity)
                // Harici filter (opsiyonel)
                .Where(_options.EntityFilter);

            // 2) Her entity tipi için, _options.ControllerTypes'te belirtilen her enum için controller yarat
            foreach (var entityType in entityTypes)
            {
                // Nihayetinde BaseEntity<TId> olduğundan, TId tipini bulalım:
                // (Artık en son kalıtım BaseEntity<T> hangi noktada ise oradan alacağız.)
                var idType = FindBaseEntityIdType(entityType);

                var controllerTypeEnum = _options.ControllerType;
                    var baseControllerType = GetBaseControllerType(controllerTypeEnum);

                if (!TryMakeGenericController(baseControllerType, entityType, idType, out var constructedController))
                    continue;

                // => Örneğin "CountryController"
                var controllerName = $"{entityType.Name}Controller";

                // Aynı isimde (class isminde) bir controller varsa eklemeyelim
                if (feature.Controllers.Any(c => c.Name == controllerName))
                    continue;

                // Reflection.Emit ile runtime'da controller tipini üret
                var dynamicControllerType = DefineDynamicControllerType(
                    constructedController,
                    controllerName,
                    _options.AreaName
                );

                feature.Controllers.Add(dynamicControllerType.GetTypeInfo());

                var routePrefix = string.IsNullOrEmpty(_options.AreaName)
            ? $"api/[controller]"
            : $"api/{_options.AreaName}/[controller]";
                LinqApiRegistry.Register(new DynamicApiInfo
                {
                    EntityName = entityType.Name,
                    ControllerName = controllerName,
                    RoutePrefix = routePrefix
                });

            }
        }
        private Type DefineDynamicControllerType(
            Type baseControllerType,
            string controllerName,
            string areaName)
        {
            // 1) Yeni tip oluştur
            var typeBuilder = DynamicAssemblyHolder.ControllerModuleBuilder.DefineType(
                controllerName,
                TypeAttributes.Public | TypeAttributes.Class,
                baseControllerType
            );

            // 2) "CountryController" → "Country"
            var baseName = controllerName.EndsWith("Controller")
                ? controllerName.Substring(0, controllerName.Length - "Controller".Length)
                : controllerName;

            // 3) Route string'i inşa et: "api/DynamicArea/Country" veya "api/Country"
            var routeValue = string.IsNullOrEmpty(areaName)
                ? $"api/{baseName}"
                : $"api/{areaName}/{baseName}";

            // 4) [Route("api/...")] attribute
            var routeAttrCtor = typeof(RouteAttribute).GetConstructor(new[] { typeof(string) });
            if (routeAttrCtor != null)
            {
                var routeAttrBuilder = new CustomAttributeBuilder(routeAttrCtor, new object[] { routeValue });
                typeBuilder.SetCustomAttribute(routeAttrBuilder);
            }

            // 5) [ApiController] attribute
            var apiControllerAttrCtor = typeof(ApiControllerAttribute).GetConstructor(Type.EmptyTypes);
            if (apiControllerAttrCtor != null)
            {
                var apiControllerAttrBuilder = new CustomAttributeBuilder(apiControllerAttrCtor, new object[] { });
                typeBuilder.SetCustomAttribute(apiControllerAttrBuilder);
            }

            // 6) Base constructor kopyala
            var baseCtor = baseControllerType.GetConstructors().FirstOrDefault();
            if (baseCtor != null)
            {
                var paramTypes = baseCtor.GetParameters().Select(p => p.ParameterType).ToArray();
                var ctorBuilder = typeBuilder.DefineConstructor(
                    MethodAttributes.Public,
                    CallingConventions.Standard,
                    paramTypes
                );
                var il = ctorBuilder.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    il.Emit(OpCodes.Ldarg, i + 1);
                }
                il.Emit(OpCodes.Call, baseCtor);
                il.Emit(OpCodes.Ret);
            }

            // 7) Tipi oluştur ve dön
            return typeBuilder.CreateType();
        }

        /// <summary>
        /// T tipi nihayetinde BaseEntity<TId>’den türediği için, o TId tipini bulur.
        /// </summary>
        private static Type FindBaseEntityIdType(Type entityType)
        {
            // Kalıtım zincirinde BaseEntity<>’yi arıyoruz
            var current = entityType;
            while (current != null && current != typeof(object))
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == typeof(BaseEntity<>))
                    return current.GetGenericArguments()[0];

                current = current.BaseType;
            }
            throw new InvalidOperationException($"Could not find BaseEntity<TId> in inheritance chain of {entityType.Name}");
        }

        /// <summary>
        /// LinqControllerType değerine göre, ilgili base generic controller tipini döndürür.
        /// </summary>
        private Type GetBaseControllerType(LinqControllerType controllerTypeEnum)
        {
            return controllerTypeEnum switch
            {
                LinqControllerType.LinqReadonlyController => typeof(LinqReadonlyController<,>),
                LinqControllerType.LinqController => typeof(LinqController<,>),
                LinqControllerType.LinqVmController => typeof(LinqVmController<,,,>),
                LinqControllerType.LinqLogController => null,
                LinqControllerType.LinqLocalizationController => null,
                _ => throw new NotSupportedException(
                    $"Unsupported LinqControllerType: {controllerTypeEnum}")
            };
        }



        /// <summary>
        /// Base generic controller tipini, entity ve id tiplerine göre kapatmaya çalışır.
        /// </summary>
        private static bool TryMakeGenericController(Type controllerTemplate, Type entityType, Type idType, out Type constructedType)
        {
            constructedType = null;
            try
            {
                var genericArgsCount = controllerTemplate.GetGenericArguments().Length;
                if (genericArgsCount == 2)
                {
                    constructedType = controllerTemplate.MakeGenericType(entityType, idType);
                    return true;
                }
                else if (genericArgsCount == 4)
                {
                    // Örnek: LinqVmController<TEntity, TId, TVm, TMapper> gibi durumlar varsa...
                    // constructedType = controllerTemplate.MakeGenericType(entityType, idType, ...);
                    // burayı ihtiyaca göre uyarlamak gerekir.
                }

                return false;
            }
            catch
            {
                return false;
            }
        }


        private static bool IsConcreteSubclassOfGenericBaseEntity(Type t)
        {
            // Önce abstract mı diye bakıyoruz
            if (t.IsAbstract) return false;

            // Kalıtım zinciri boyunca BaseEntity<TId> arıyoruz
            while (t != null && t != typeof(object))
            {
                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(BaseEntity<>))
                    return true;

                t = t.BaseType;
            }
            return false;
        }


    }

    /// <summary>
    /// Legacy or alternative feature provider that works off a ConcurrentDictionary<string, Type>.
    /// You might separate this into another file or rename it if needed.
    /// </summary>
    public class DynamicLinqApiControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly ConcurrentDictionary<string, Type> _entities;
        public string AreaName { get; }

        public DynamicLinqApiControllerFeatureProvider(ConcurrentDictionary<string, Type> entities, string areaName)
        {
            _entities = entities;
            AreaName = areaName;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var kvp in _entities)
            {
                var fullKey = kvp.Key; // "schema.table" veya sadece "table"
                var entityType = kvp.Value;
                var idType = entityType.BaseType?.GetGenericArguments()[0] ?? typeof(long);

                var baseControllerType = typeof(LinqController<,>).MakeGenericType(entityType, idType);

                var partsArr = fullKey.Split('.');
                var schema = partsArr.Length > 1 ? partsArr[0] : "dbo";
                var table = partsArr.Length > 1 ? partsArr[1] : partsArr[0];

                var controllerName = (schema.ToLower() == "dbo" ? table : $"{schema}_{table}");

                if (feature.Controllers.Any(c => c.Name == controllerName))
                    continue;

                var customControllerType = DefineDynamicControllerType(baseControllerType, controllerName, AreaName);
                feature.Controllers.Add(customControllerType.GetTypeInfo());


                LinqApiRegistry.Register(new DynamicApiInfo
                {
                    EntityName = controllerName.Replace("Controller", string.Empty),
                    ControllerName = controllerName.Replace("Controller",string.Empty),
                    RoutePrefix = AreaName
                });
            }
              
        }
        private Type DefineDynamicControllerType(
            Type baseControllerType,
            string controllerName,
            string areaName)
        {
            // 1) Yeni tip oluştur
            var typeBuilder = DynamicAssemblyHolder.ControllerModuleBuilder.DefineType(
                controllerName,
                TypeAttributes.Public | TypeAttributes.Class,
                baseControllerType
            );

            // 2) "CountryController" → "Country"
            var baseName = controllerName.EndsWith("Controller")
                ? controllerName.Substring(0, controllerName.Length - "Controller".Length)
                : controllerName;

            // 3) Route string'i inşa et: "api/DynamicArea/Country" veya "api/Country"
            var routeValue = string.IsNullOrEmpty(areaName)
                ? $"api/{baseName}"
                : $"api/{areaName}/{baseName}";

            // 4) [Route("api/...")] attribute
            var routeAttrCtor = typeof(RouteAttribute).GetConstructor(new[] { typeof(string) });
            if (routeAttrCtor != null)
            {
                var routeAttrBuilder = new CustomAttributeBuilder(routeAttrCtor, new object[] { routeValue });
                typeBuilder.SetCustomAttribute(routeAttrBuilder);
            }

            // 5) [ApiController] attribute
            var apiControllerAttrCtor = typeof(ApiControllerAttribute).GetConstructor(Type.EmptyTypes);
            if (apiControllerAttrCtor != null)
            {
                var apiControllerAttrBuilder = new CustomAttributeBuilder(apiControllerAttrCtor, new object[] { });
                typeBuilder.SetCustomAttribute(apiControllerAttrBuilder);
            }

            // 6) Base constructor kopyala
            var baseCtor = baseControllerType.GetConstructors().FirstOrDefault();
            if (baseCtor != null)
            {
                var paramTypes = baseCtor.GetParameters().Select(p => p.ParameterType).ToArray();
                var ctorBuilder = typeBuilder.DefineConstructor(
                    MethodAttributes.Public,
                    CallingConventions.Standard,
                    paramTypes
                );
                var il = ctorBuilder.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                for (int i = 0; i < paramTypes.Length; i++)
                {
                    il.Emit(OpCodes.Ldarg, i + 1);
                }
                il.Emit(OpCodes.Call, baseCtor);
                il.Emit(OpCodes.Ret);
            }

            // 7) Tipi oluştur ve dön
            return typeBuilder.CreateType();
        }

    }
}
