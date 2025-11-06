using LinqApi.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LinqApi.Controller // isimlendirme sana kalmış
{
    /// <summary>
    /// Verilen DbContext içindeki DbSet<TEntity> tiplerini tarar,
    /// BaseEntity<TId>'den türeyen entity'ler için
    /// LinqController<TEntity, TId> kapalı generic controller'larını
    /// MVC ControllerFeature içine enjekte eder.
    /// </summary>
    public class LinqControllerFeatureProvider<TDbContext>
        : IApplicationFeatureProvider<ControllerFeature>
        where TDbContext : DbContext
    {
        public void PopulateFeature(
            IEnumerable<ApplicationPart> parts,
            ControllerFeature feature)
        {
            // 1. DbContext içindeki tüm public DbSet<TEntity> property'lerini bul
            var entityTypes = typeof(TDbContext)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p =>
                    p.PropertyType.IsGenericType &&
                    p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
                .Select(p => p.PropertyType.GetGenericArguments()[0]) // TEntity
                .Distinct()
                .ToList();

            foreach (var entityType in entityTypes)
            {
                // 2. Bu tip BaseEntity<TId> tabanlı mı? değilse geç
                if (!IsBasedOnBaseEntity(entityType))
                    continue;

                // 3. Id tipini bul (BaseEntity<TId>'den)
                var idType = GetIdTypeFromBaseEntity(entityType);

                // 4. Generic controller'ı kapat: LinqController<TEntity,TId>
                var closedControllerType = typeof(LinqController<,>)
                    .MakeGenericType(entityType, idType);

                var controllerTypeInfo = closedControllerType.GetTypeInfo();

                // 5. Daha önce eklenmiş mi kontrol et
                if (!feature.Controllers.Contains(controllerTypeInfo))
                {
                    feature.Controllers.Add(controllerTypeInfo);
                }
            }
        }

        private static bool IsBasedOnBaseEntity(Type entityType)
        {
            var t = entityType;
            while (t != null && t != typeof(object))
            {
                if (t.IsGenericType &&
                    t.GetGenericTypeDefinition() == typeof(BaseEntity<>))
                    return true;

                t = t.BaseType!;
            }
            return false;
        }

        /// <summary>
        /// BaseEntity&lt;TId&gt;'deki TId tipini bul.
        /// Bulamazsa default int döner (senin repo zaten öyle davranıyordu).
        /// </summary>
        private static Type GetIdTypeFromBaseEntity(Type entityType)
        {
            var t = entityType;
            while (t != null && t != typeof(object))
            {
                if (t.IsGenericType &&
                    t.GetGenericTypeDefinition() == typeof(BaseEntity<>))
                {
                    return t.GetGenericArguments()[0];
                }

                t = t.BaseType!;
            }

            return typeof(int);
        }
    }


    public class LinqRouteConvention : IApplicationModelConvention
    {
        private readonly string _prefix;
        private readonly string _suffix;

        public LinqRouteConvention(string prefix, string suffix)
        {
            _prefix = (prefix ?? "").Trim('/'); // "api/admin"
            _suffix = suffix ?? "";              // "s" falan
        }

        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                var controllerType = controller.ControllerType.AsType();

                // Sadece bizim dinamik LinqController<TEntity,TId> kapalı tiplerimizi hedefle
                if (!controllerType.IsGenericType) continue;

                var genericDef = controllerType.GetGenericTypeDefinition();
                if (genericDef != typeof(LinqController<,>)) continue;

                // TEntity tipinin adını al
                var entityType = controllerType.GetGenericArguments()[0];
                var entityName = entityType.Name;

                var finalSegment = entityName + _suffix;
                var fullRoute = string.IsNullOrWhiteSpace(_prefix)
                    ? finalSegment            // "Tours"
                    : $"{_prefix}/{finalSegment}"; // "api/admin/Tours"

                if (controller.Selectors == null || controller.Selectors.Count == 0)
                {
                    controller.Selectors.Add(new SelectorModel());
                }

                foreach (var selector in controller.Selectors)
                {
                    selector.AttributeRouteModel = new AttributeRouteModel(
                        new RouteAttribute(fullRoute)
                    );
                }

                // Swagger grubu istersen:
                controller.ApiExplorer.GroupName = string.IsNullOrWhiteSpace(_prefix)
                    ? "default"
                    : _prefix.Replace("/", "_");
            }
        }
    }
}