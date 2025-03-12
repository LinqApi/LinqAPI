//using LinqApi.Repository;
//using Microsoft.EntityFrameworkCore;
//using Microsoft.Extensions.DependencyInjection;
//using System.Collections.Concurrent;

//namespace LinqApi.Helpers
//{
//    public static class RepositoryHelper
//    {
//        public static void AddRepositories<TRepository>(IServiceCollection services, Type dbContextType)
//        {
//            foreach (var entity in entities)
//            {
//                var idType = entity.Value.BaseType?.GetGenericArguments()[0] ?? typeof(long);
//                var repoType = typeof(LinqRepository<,,>).MakeGenericType(dbContextType, entity.Value, idType);
//                var interfaceType = typeof(ILinqRepository<,>).MakeGenericType(entity.Value, idType);
//                services.AddScoped(interfaceType, repoType);
//            }
//        }

//    }



//}
