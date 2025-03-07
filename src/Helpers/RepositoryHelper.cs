using LinqApi.Context;
using LinqApi.Repository;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace LinqApi.Helpers
{
    public static class RepositoryHelper
    {
        public static void AddRepositories(IServiceCollection services, ConcurrentDictionary<string, Type> entities)
        {
            foreach (var entity in entities)
            {
                var idType = entity.Value.BaseType?.GetGenericArguments()[0] ?? typeof(long);
                var repoType = typeof(LinqRepository<,,>).MakeGenericType(typeof(DynamicDbContext), entity.Value, idType);
                var interfaceType = typeof(ILinqRepository<,>).MakeGenericType(entity.Value, idType);
                services.AddScoped(interfaceType, repoType);
            }
        }
    }
}
