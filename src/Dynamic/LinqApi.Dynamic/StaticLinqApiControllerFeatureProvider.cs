namespace LinqApi.Extensions
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.ApplicationParts;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.EntityFrameworkCore;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection.Emit;
    using System.Reflection;

    namespace LinqApi.Controller
    {
        /// <summary>
        /// A custom application feature provider that dynamically registers API controllers for each DbSet property
        /// found on the specified DbContext. This provider closes a generic base controller type using the entity type
        /// and its identifier type. The generated controllers are assigned to the specified area.
        /// </summary>
        public class StaticLinqApiControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
        {
            private readonly Type _dbContextType;
            private readonly string _areaName;
            private readonly Type _baseControllerType;
            private readonly ModuleBuilder? _moduleBuilder;

            /// <summary>
            /// Initializes a new instance of the <see cref="StaticLinqApiControllerFeatureProvider"/> class
            /// using a provided ModuleBuilder for dynamic type creation.
            /// </summary>
            /// <param name="dbContextType">
            /// The <see cref="DbContext"/> type to scan for DbSet properties.
            /// </param>
            /// <param name="areaName">
            /// The area name to assign to the generated controllers.
            /// </param>
            /// <param name="baseControllerType">
            /// The open generic base controller type to close for each entity (e.g. typeof(LinqController&lt;,>)).
            /// </param>
            /// <param name="moduleBuilder">
            /// The <see cref="ModuleBuilder"/> used for dynamic type creation. If null, dynamic creation is skipped.
            /// </param>
            public StaticLinqApiControllerFeatureProvider(
                Type dbContextType,
                string areaName,
                Type baseControllerType,
                ModuleBuilder? moduleBuilder = null)
            {
                _dbContextType = dbContextType ?? throw new ArgumentNullException(nameof(dbContextType));
                _areaName = areaName ?? throw new ArgumentNullException(nameof(areaName));
                _baseControllerType = baseControllerType ?? throw new ArgumentNullException(nameof(baseControllerType));
                _moduleBuilder = moduleBuilder;
            }

            /// <summary>
            /// Populates the controller feature with dynamically generated controllers.
            /// Each controller is created for every DbSet property found on the specified DbContext.
            /// </summary>
            /// <param name="parts">The application parts.</param>
            /// <param name="feature">The controller feature to populate.</param>
            public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
            {
                // Retrieve all DbSet properties from the specified DbContext.
                var dbSetProps = _dbContextType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Where(p => p.PropertyType.IsGenericType &&
                                p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

                foreach (var prop in dbSetProps)
                {
                    // Retrieve the entity type from the DbSet<>
                    var entityType = prop.PropertyType.GetGenericArguments().First();
                    // Get the identifier type from the entity's base type (assumes entity inherits from BaseEntity<TId>)
                    var idType = GetEntityIdType(entityType);

                    // Close the generic base controller using the entity and id types.
                    var controllerType = _baseControllerType.MakeGenericType(entityType, idType);
                    // Generate a unique controller name (e.g. "Dashboard_CountryController")
                    var controllerName = GetControllerName(entityType);

                    // Avoid duplicate controller registrations.
                    if (!feature.Controllers.Any(c => c.Name == controllerName))
                    {
                        var customControllerType = BuildCustomControllerType(controllerType, controllerName);
                        feature.Controllers.Add(customControllerType.GetTypeInfo());
                    }
                }
            }

            /// <summary>
            /// Retrieves the identifier type from the entity's base type.
            /// If the entity does not have a generic base type, it defaults to <see cref="long"/>.
            /// </summary>
            /// <param name="entityType">The entity type.</param>
            /// <returns>The identifier type.</returns>
            private Type GetEntityIdType(Type entityType)
            {
                if (entityType.BaseType?.IsGenericType == true)
                {
                    // Assumes the entity inherits from BaseEntity<TId>
                    return entityType.BaseType.GetGenericArguments().First();
                }
                return typeof(long);
            }

            /// <summary>
            /// Generates a controller name in the format "{AreaName}_{EntityName}Controller".
            /// </summary>
            /// <param name="entityType">The entity type.</param>
            /// <returns>The generated controller name.</returns>
            private string GetControllerName(Type entityType)
            {
                return $"{_areaName}_{entityType.Name}Controller";
            }

            /// <summary>
            /// Creates a custom controller type. If a <see cref="ModuleBuilder"/> is provided, a dynamic type is created,
            /// and the [Area] attribute is applied. Otherwise, the closed base controller type is returned directly.
            /// </summary>
            /// <param name="baseControllerType">The closed generic base controller type.</param>
            /// <param name="customName">The custom controller name.</param>
            /// <returns>The custom controller type.</returns>
            private Type BuildCustomControllerType(Type baseControllerType, string customName)
            {
                if (_moduleBuilder != null)
                {
                    var typeBuilder = _moduleBuilder.DefineType(
                        customName,
                        TypeAttributes.Public | TypeAttributes.Class,
                        baseControllerType);

                    // Add the [Area] attribute so that the controller is associated with the specified area.
                    var areaAttrCtor = typeof(AreaAttribute).GetConstructor(new[] { typeof(string) });
                    var areaAttrBuilder = new CustomAttributeBuilder(areaAttrCtor, new object[] { _areaName });
                    typeBuilder.SetCustomAttribute(areaAttrBuilder);

                    return typeBuilder.CreateType();
                }
                else
                {
                    // If dynamic type creation is not used, simply return the closed base controller type.
                    return baseControllerType;
                }
            }
        }

    }

}
