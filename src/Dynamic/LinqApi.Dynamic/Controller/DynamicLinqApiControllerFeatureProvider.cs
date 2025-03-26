using LinqApi.Controller;
using LinqApi.Dynamic.Assembly;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;
using System.Reflection.Emit;
using System.Reflection;

namespace LinqApi.Dynamic.Controller
{
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
                string fullKey = kvp.Key; // "schema.table" ya da sadece "table"
                var entityType = kvp.Value;
                var idType = entityType.BaseType?.GetGenericArguments()[0] ?? typeof(long);
                var controllerType = typeof(LinqController<,>).MakeGenericType(entityType, idType);

                var partsArr = fullKey.Split('.');

                string schema = partsArr.Length > 1 ? partsArr[0] : "dbo";
                string table = partsArr.Length > 1 ? partsArr[1] : partsArr[0];

                string controllerName = (schema.ToLower() == "dbo" ? table : $"{schema}_{table}") + "Controller";

                if (feature.Controllers.Any(c => c.Name == controllerName))
                    continue;

                var customControllerType = BuildCustomControllerType(controllerType, controllerName);
                feature.Controllers.Add(customControllerType.GetTypeInfo());
            }
        }

        private Type BuildCustomControllerType(Type baseControllerType, string customName)
        {
            var typeBuilder = CreateTypeBuilder(customName);
            typeBuilder.SetParent(baseControllerType);

            // Area attribute ekleyelim:
            var areaAttrCtor = typeof(AreaAttribute).GetConstructor(new[] { typeof(string) });
            var areaAttrBuilder = new CustomAttributeBuilder(areaAttrCtor, new object[] { this.AreaName });
            typeBuilder.SetCustomAttribute(areaAttrBuilder);

            var baseCtor = baseControllerType.GetConstructors().FirstOrDefault();
            if (baseCtor != null)
            {
                var paramTypes = baseCtor.GetParameters().Select(p => p.ParameterType).ToArray();
                var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, paramTypes);
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

        private TypeBuilder CreateTypeBuilder(string controllerName) => DynamicAssemblyHolder.ControllerModuleBuilder.DefineType(controllerName, TypeAttributes.Public | TypeAttributes.Class, typeof(ControllerBase));
    }
}
