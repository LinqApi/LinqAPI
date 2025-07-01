using LinqApi.Repository;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace LinqApi.Dynamic.Extensions
{
    public static class DynamicRepositoryHelper
    {
        public static void AddRepositories(IServiceCollection services, ConcurrentDictionary<string, Type> entities, Type dbContextType)
        {
            foreach (var entity in entities)
            {
                var idType = entity.Value.BaseType?.GetGenericArguments()[0] ?? typeof(long);
                var repoType = typeof(LinqRepository<,,>).MakeGenericType(dbContextType, entity.Value, idType);
                var interfaceType = typeof(ILinqRepository<,>).MakeGenericType(entity.Value, idType);
                services.AddScoped(interfaceType, repoType);
            }
        }

    }
}
