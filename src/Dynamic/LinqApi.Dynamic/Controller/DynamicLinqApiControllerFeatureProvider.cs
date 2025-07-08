using LinqApi.Dynamic.Assembly;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Reflection.Emit;
using System.Reflection;
using LinqApi.Controller;
using LinqApi.Logging;
using Microsoft.EntityFrameworkCore;



namespace LinqApi.Dynamic.Controller
{
    public class LinqApiApmOptions
    {
        /// <summary>
        /// Specifies the DbContext type whose DbSet properties are used to create dynamic controllers.
        /// </summary>
        public Type DbContextType { get; set; }

        /// <summary>
        /// Optionally used to filter entity types (if you do not want controllers for some entities).
        /// Defaults to a function that always returns true.
        /// </summary>
        public Func<Type, bool> EntityFilter { get; set; } = type => true;

        /// <summary>
        /// The controller type to be generated (for example, LinqController or LinqReadonlyController).
        /// </summary>
        public LinqControllerType ControllerType { get; set; } = LinqControllerType.LinqController;

        /// <summary>
        /// The area name for the generated controllers.
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
        private static readonly Dictionary<string, List<Type>> _extensions = new Dictionary<string, List<Type>>();

        public static IReadOnlyList<DynamicApiInfo> Apis => _apis;

        public static void Register(DynamicApiInfo info)
        {
            _apis.Add(info);
        }

        public static void RegisterRange(IEnumerable<DynamicApiInfo> infos)
        {
            _apis.AddRange(infos);
        }

        // Register an extension type for a given controller name.
        public static void RegisterExtension(string controllerName, Type extensionType)
        {
            if (!_extensions.ContainsKey(controllerName))
            {
                _extensions[controllerName] = new List<Type>();
            }
            _extensions[controllerName].Add(extensionType);
        }

        // Expose the registered extensions (if needed for later processing)
        public static IReadOnlyDictionary<string, List<Type>> Extensions => _extensions;
    }

    public class DynamicApiInfo
    {
        public string EntityName { get; set; }
        public string ControllerName { get; set; }
        public string RoutePrefix { get; set; } // e.g., "api/[controller]"
        public string AreaName { get; set; }    // e.g., "DynamicArea"
        public List<object> Properties { get;  set; }
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


            // 2) Find entity types from the DbContext's DbSet<> properties.
            var entityTypes = _options.DbContextType
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                    p.PropertyType.IsGenericType &&
                    p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>)
                )
                .Select(p => p.PropertyType.GetGenericArguments()[0])
                .Where(IsConcreteSubclassOfGenericBaseEntity)
                .Where(_options.EntityFilter);

            // 3) Generate a dynamic controller for each entity.
            foreach (var entityType in entityTypes)
            {
                // Get the ID type from the inheritance chain of BaseEntity<TId>.
                var idType = FindBaseEntityIdType(entityType);

                var controllerTypeEnum = _options.ControllerType;
                var baseControllerType = GetBaseControllerType(controllerTypeEnum);
                if (!TryMakeGenericController(baseControllerType, entityType, idType, out var constructedController))
                    continue;

                // Construct the controller name, e.g., "CountryController"
                var controllerName = $"{entityType.Name}Controller";

                // If a controller with the same name already exists, skip.
                if (feature.Controllers.Any(c => c.Name == controllerName))
                    continue;

                // Use Reflection.Emit to generate a dynamic controller type.
                var dynamicControllerType = DefineDynamicControllerType(
                    constructedController,
                    controllerName,
                    _options.AreaName
                );

                feature.Controllers.Add(dynamicControllerType.GetTypeInfo());

                var routePrefix = string.IsNullOrEmpty(_options.AreaName)
                    ? $"api/[controller]"
                    : $"api/{_options.AreaName}/[controller]";

                var propertySchema = ViewModelSchemaHelper.GetSchema(entityType);

                LinqApiRegistry.Register(new DynamicApiInfo
                {
                    EntityName = entityType.Name,
                    ControllerName = controllerName,
                    RoutePrefix = routePrefix,
                    AreaName = _options.AreaName,
                    Properties = propertySchema

                });
            }
        }

        private Type DefineDynamicControllerType(
            Type baseControllerType,
            string controllerName,
            string areaName)
        {
            // 1) Create a new type.
            var typeBuilder = DynamicAssemblyHolder.ControllerModuleBuilder.DefineType(
                controllerName,
                TypeAttributes.Public | TypeAttributes.Class,
                baseControllerType
            );

            // 2) Remove the "Controller" suffix from the name (if present).
            var baseName = controllerName.EndsWith("Controller")
                ? controllerName.Substring(0, controllerName.Length - "Controller".Length)
                : controllerName;

            // 3) Build the route string: "api/DynamicArea/Country" or "api/Country".
            var routeValue = string.IsNullOrEmpty(areaName)
                ? $"api/{baseName}"
                : $"api/{areaName}/{baseName}";

            // 4) Apply the [Route("...")] attribute.
            var routeAttrCtor = typeof(RouteAttribute).GetConstructor(new[] { typeof(string) });
            if (routeAttrCtor != null)
            {
                var routeAttrBuilder = new CustomAttributeBuilder(routeAttrCtor, new object[] { routeValue });
                typeBuilder.SetCustomAttribute(routeAttrBuilder);
            }

            // 5) Apply the [ApiController] attribute.
            var apiControllerAttrCtor = typeof(ApiControllerAttribute).GetConstructor(Type.EmptyTypes);
            if (apiControllerAttrCtor != null)
            {
                var apiControllerAttrBuilder = new CustomAttributeBuilder(apiControllerAttrCtor, new object[] { });
                typeBuilder.SetCustomAttribute(apiControllerAttrBuilder);
            }

            // 6) Copy the base constructor.
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

            // 7) Create and return the type.
            return typeBuilder.CreateType();
        }

        /// <summary>
        /// Finds the ID type (TId) from the inheritance chain of BaseEntity&lt;TId&gt; for a given entity type.
        /// </summary>
        private static Type FindBaseEntityIdType(Type entityType)
        {
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
        /// Returns the corresponding base generic controller type based on the LinqControllerType enumeration.
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
                _ => throw new NotSupportedException($"Unsupported LinqControllerType: {controllerTypeEnum}")
            };
        }

        /// <summary>
        /// Attempts to create a closed generic controller type based on the provided template.
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
                    // Example: LinqVmController<TEntity, TId, TVm, TMapper> – adjust accordingly if needed.
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
            if (t.IsAbstract) return false;
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
    /// Legacy or alternative feature provider using a ConcurrentDictionary.
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
                var fullKey = kvp.Key; // "schema.table" or just "table"
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
                var propertySchema = ViewModelSchemaHelper.GetSchema(entityType);
                LinqApiRegistry.Register(new DynamicApiInfo
                {
                    EntityName = controllerName.Replace("Controller", string.Empty),
                    ControllerName = controllerName.Replace("Controller", string.Empty),
                    RoutePrefix = AreaName,
                    Properties = propertySchema

                });
            }
        }

        private Type DefineDynamicControllerType(
            Type baseControllerType,
            string controllerName,
            string areaName)
        {
            var typeBuilder = DynamicAssemblyHolder.ControllerModuleBuilder.DefineType(
                controllerName,
                TypeAttributes.Public | TypeAttributes.Class,
                baseControllerType
            );

            var baseName = controllerName.EndsWith("Controller")
                ? controllerName.Substring(0, controllerName.Length - "Controller".Length)
                : controllerName;

            var routeValue = string.IsNullOrEmpty(areaName)
                ? $"api/{baseName}"
                : $"api/{areaName}/{baseName}";

            var routeAttrCtor = typeof(RouteAttribute).GetConstructor(new[] { typeof(string) });
            if (routeAttrCtor != null)
            {
                var routeAttrBuilder = new CustomAttributeBuilder(routeAttrCtor, new object[] { routeValue });
                typeBuilder.SetCustomAttribute(routeAttrBuilder);
            }

            var apiControllerAttrCtor = typeof(ApiControllerAttribute).GetConstructor(Type.EmptyTypes);
            if (apiControllerAttrCtor != null)
            {
                var apiControllerAttrBuilder = new CustomAttributeBuilder(apiControllerAttrCtor, new object[] { });
                typeBuilder.SetCustomAttribute(apiControllerAttrBuilder);
            }

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

            return typeBuilder.CreateType();
        }
    }
}
