using LinqApi.Controller;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.EntityFrameworkCore;
using System.Reflection;
using System.Reflection.Emit;

public class StaticLinqApiControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
{
    private readonly Type _dbContextType;
    private readonly string _areaName;
    private readonly ModuleBuilder? _moduleBuilder; // Opsiyonel hale getiriyoruz.

    // Eski Constructor (ModuleBuilder ile)
    public StaticLinqApiControllerFeatureProvider(Type dbContextType, string areaName, ModuleBuilder moduleBuilder)
    {
        _dbContextType = dbContextType;
        _areaName = areaName;
        _moduleBuilder = moduleBuilder;
    }

    // Yeni Constructor (ModuleBuilder olmadan da kullanılabilir)
    public StaticLinqApiControllerFeatureProvider(Type dbContextType, string areaName)
    {
        _dbContextType = dbContextType;
        _areaName = areaName;
        _moduleBuilder = null; // ModuleBuilder sağlanmadıysa null olur.
    }

    public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
    {
        var dbContextProps = _dbContextType.GetProperties()
            .Where(p => p.PropertyType.IsGenericType &&
                        p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));

        foreach (var prop in dbContextProps)
        {
            var entityType = prop.PropertyType.GetGenericArguments().First();
            var idType = entityType.BaseType?.GetGenericArguments().First() ?? typeof(long);

            var controllerType = typeof(LinqController<,>).MakeGenericType(entityType, idType);
            var controllerName = $"{_areaName}_{entityType.Name}Controller";

            if (!feature.Controllers.Any(c => c.Name == controllerName))
            {
                var customControllerType = BuildCustomControllerType(controllerType, controllerName);
                feature.Controllers.Add(customControllerType.GetTypeInfo());
            }
        }
    }

    private Type BuildCustomControllerType(Type baseControllerType, string customName)
    {
        if (_moduleBuilder != null)
        {
            // Eğer ModuleBuilder varsa dinamik oluştur.
            var typeBuilder = _moduleBuilder.DefineType(customName, TypeAttributes.Public | TypeAttributes.Class, baseControllerType);

            // Area attribute ekleyelim:
            var areaAttrCtor = typeof(AreaAttribute).GetConstructor(new[] { typeof(string) });
            var areaAttrBuilder = new CustomAttributeBuilder(areaAttrCtor, new object[] { _areaName });
            typeBuilder.SetCustomAttribute(areaAttrBuilder);

            return typeBuilder.CreateType();
        }
        else
        {
            // Eğer ModuleBuilder yoksa, doğrudan baseControllerType kullan.
            return baseControllerType;
        }
    }
}
