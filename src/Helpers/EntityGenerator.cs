using LinqApi.Model;
using System.Reflection;
using System.Reflection.Emit;

namespace LinqApi.Helpers
{
    public static class EntityGenerator
    {
        // entityName: benzersiz isim (örneğin, "AppLoginType" veya "sales_Orders")
        public static Type GenerateEntity(string schema, string tableName, KeyValuePair<string, Type> primaryKey, Dictionary<string, Type> columns, string entityName)
        {
            // Tek bir dinamik assembly üzerinden tip oluşturuluyor
            TypeBuilder typeBuilder = DynamicAssemblyHolder.EntityModuleBuilder.DefineType($"Entities.{entityName}",
                TypeAttributes.Public | TypeAttributes.Class, typeof(BaseEntity<>).MakeGenericType(primaryKey.Value));

            string pkColumnName = primaryKey.Key;

            foreach (var column in columns)
            {
                string colName = column.Key;
                Type colType = column.Value;
                if (colName == pkColumnName)
                    colName = "Id";

                FieldBuilder field = typeBuilder.DefineField($"_{colName}", colType, FieldAttributes.Private);
                PropertyBuilder propBuilder = typeBuilder.DefineProperty(colName, PropertyAttributes.HasDefault, colType, null);

                MethodAttributes methodAttrs = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;
                // Getter metodu
                MethodBuilder getter = typeBuilder.DefineMethod($"get_{colName}", methodAttrs, colType, Type.EmptyTypes);
                ILGenerator getterIL = getter.GetILGenerator();
                getterIL.Emit(OpCodes.Ldarg_0);
                getterIL.Emit(OpCodes.Ldfld, field);
                getterIL.Emit(OpCodes.Ret);
                propBuilder.SetGetMethod(getter);

                // Setter metodu
                MethodBuilder setter = typeBuilder.DefineMethod($"set_{colName}", methodAttrs, null, new Type[] { colType });
                ILGenerator setterIL = setter.GetILGenerator();
                setterIL.Emit(OpCodes.Ldarg_0);
                setterIL.Emit(OpCodes.Ldarg_1);
                setterIL.Emit(OpCodes.Stfld, field);
                setterIL.Emit(OpCodes.Ret);
                propBuilder.SetSetMethod(setter);
            }

            return typeBuilder.CreateType();
        }
    }
}
