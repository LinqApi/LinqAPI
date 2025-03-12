using LinqApi.Helpers;
using LinqApi.Model;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Reflection.Emit;
using System.Reflection;
using LinqApi.Dynamic.Assembly;

namespace LinqApi.Dynamic.Context
{
    public static class DynamicDbContextGenerator
    {
        public static Type GenerateDbContextType(string areaName)
        {
            string typeName = $"DynamicDbContext_{areaName}";
            var typeBuilder = DynamicAssemblyHolder.EntityModuleBuilder.DefineType(
                $"DynamicEntities.{typeName}",
                TypeAttributes.Public | TypeAttributes.Class,
                typeof(DynamicDbContext) // Base sınıfımız
            );

            // Artık base constructor'ın ilk parametresi non-generic DbContextOptions'dur.
            Type[] ctorParams = new Type[]
            {
            typeof(DbContextOptions),
            typeof(ConcurrentDictionary<string, Type>),
            typeof(ConcurrentDictionary<string, string>),
            typeof(Dictionary<string, Dictionary<string, ColumnDefinition>>)
            };

            var baseCtor = typeof(DynamicDbContext).GetConstructor(
                BindingFlags.Public | BindingFlags.Instance,
                null,
                ctorParams,
                null
            );
            if (baseCtor == null)
            {
                throw new InvalidOperationException("DynamicDbContext'in uygun constructor'ı bulunamadı.");
            }

            // Türetilen sınıfa aynı parametreleri alan constructor ekliyoruz.
            ConstructorBuilder ctorBuilder = typeBuilder.DefineConstructor(
                MethodAttributes.Public,
                CallingConventions.Standard,
                ctorParams
            );

            ILGenerator il = ctorBuilder.GetILGenerator();
            // this
            il.Emit(OpCodes.Ldarg_0);
            // options (arg1)
            il.Emit(OpCodes.Ldarg_1);
            // dynamicEntities (arg2)
            il.Emit(OpCodes.Ldarg_2);
            // primaryKeyMappings (arg3)
            il.Emit(OpCodes.Ldarg_3);
            // columnSchemas (arg4)
            il.Emit(OpCodes.Ldarg_S, (byte)4);
            il.Emit(OpCodes.Call, baseCtor);
            il.Emit(OpCodes.Ret);

            return typeBuilder.CreateType();
        }
    }
}
