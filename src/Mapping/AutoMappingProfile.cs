using AutoMapper;
using LinqApi.Controller;

public class AutoLinqMappingProfile : Profile
{
    public AutoLinqMappingProfile()
    {
        // Tüm assembly'lerdeki LinqController türevlerini tara.
        var controllerTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && IsLinqController(t));

        foreach (var controllerType in controllerTypes)
        {
            // LinqController<T1, T2, TId> base tipinden generic argümanları çek.
            var baseType = controllerType.BaseType;
            if (baseType != null && baseType.IsGenericType &&
                baseType.GetGenericTypeDefinition() == typeof(LinqController<,,>))
            {
                var genericArgs = baseType.GetGenericArguments();
                var entityType = genericArgs[0];
                var dtoType = genericArgs[1];

                // Otomatik mapping oluştur.
                CreateMap(entityType, dtoType).ReverseMap();
            }
        }
    }

    private bool IsLinqController(Type type)
    {
       
        var baseType = type.BaseType;
        while (baseType != null)
        {
            if (baseType.IsGenericType && baseType.GetGenericTypeDefinition() == typeof(LinqController<,,>))
                return true;
            baseType = baseType.BaseType;
        }
        return false;
    }
}
