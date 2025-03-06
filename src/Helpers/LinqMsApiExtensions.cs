using LinqApi.Controller;
using LinqApi.Model;
using LinqApi.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinqApi.Helpers
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddLinqMsApi(this IServiceCollection services, string connectionString)
        {
            // Tüm tablo (schema, table) bilgilerini toplu sorguyla alalım
            var tables = DatabaseHelper.GetAllTablesWithSchema(connectionString);
            var dynamicEntities = new Dictionary<string, Type>();
            var primaryKeyMappings = new Dictionary<string, string>();

            // Toplu sorgular: tüm tablo şema bilgileri ve primary key bilgilerini alalım
            var allTableSchemas = DatabaseHelper.GetAllTableSchemas(connectionString);
            var allPrimaryKeys = DatabaseHelper.GetAllPrimaryKeys(connectionString);

            Parallel.ForEach(tables, tableInfo =>
            {
                var (schema, table) = tableInfo;
                string key = $"{schema}.{table}";
                if (!allTableSchemas.TryGetValue(key, out Dictionary<string, Type> columns))
                    return;
                if (!allPrimaryKeys.TryGetValue(key, out string pkColumn))
                    return;
                if (!columns.TryGetValue(pkColumn, out Type pkType))
                    return;
                if (!IsValidPrimaryKey(pkType))
                    return;

                var primaryKeyPair = new KeyValuePair<string, Type>(pkColumn, pkType);
                // Eğer schema "dbo" ise entity adı sadece tablo adı, değilse "schema_tablename"
                string entityName = (schema.ToLower() == "dbo" ? table : $"{schema}_{table}");
                var entityType = EntityGenerator.GenerateEntity(schema, table, primaryKeyPair, columns, entityName);
                lock (dynamicEntities)
                {
                    dynamicEntities.Add(key, entityType);
                    primaryKeyMappings.Add(key, pkColumn);
                }
            });

            services.AddSingleton(dynamicEntities);
            services.AddSingleton(primaryKeyMappings);

            services.AddDbContext<DynamicDbContext>((sp, options) =>
            {
                options.UseSqlServer(connectionString);
            });

            var featureProvider = new DynamicLinqmsControllerFeatureProvider(dynamicEntities);
            services.AddSingleton(featureProvider);
            services.AddSingleton<IApplicationFeatureProvider<ControllerFeature>>(featureProvider);

            RepositoryHelper.AddRepositories(services, dynamicEntities);

            return services;
        }

        private static bool IsValidPrimaryKey(Type keyType)
        {
            return keyType == typeof(int) ||
                   keyType == typeof(long) ||
                   keyType == typeof(string) ||
                   keyType == typeof(Guid) ||
                   keyType == typeof(DateTime) ||
                   keyType == typeof(short);
        }
    }

    public class DynamicDbContext : DbContext
    {
        private readonly Dictionary<string, Type> _dynamicEntities;
        private readonly Dictionary<string, string> _primaryKeyMappings;

        public DynamicDbContext(DbContextOptions<DynamicDbContext> options,
                                Dictionary<string, Type> dynamicEntities,
                                Dictionary<string, string> primaryKeyMappings)
            : base(options)
        {
            _dynamicEntities = dynamicEntities;
            _primaryKeyMappings = primaryKeyMappings;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            foreach (var entity in _dynamicEntities)
            {
                var entityType = entity.Value;
                var builder = modelBuilder.Entity(entityType);
                // key format: "schema.table"
                var parts = entity.Key.Split('.');
                string schema = parts[0];
                string table = parts[1];
                // EF’ye tablo ve şema bilgisini veriyoruz
                builder.ToTable(table, schema);

                if (_primaryKeyMappings.TryGetValue(entity.Key, out string primaryKeyColumnName))
                {
                    builder.HasKey("Id");
                    builder.Property("Id").HasColumnName(primaryKeyColumnName);
                }
            }
        }
    }

    public static class EntityGenerator
    {
        // entityName: benzersiz isim (örneğin, "AppLoginType" veya "sales_Orders")
        public static Type GenerateEntity(string schema, string tableName, KeyValuePair<string, Type> primaryKey, Dictionary<string, Type> columns, string entityName)
        {
            AssemblyName asmName = new("DynamicEntities");
            AssemblyBuilder asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            ModuleBuilder modBuilder = asmBuilder.DefineDynamicModule("MainModule");

            TypeBuilder typeBuilder = modBuilder.DefineType($"Entities.{entityName}",
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
                MethodBuilder getter = typeBuilder.DefineMethod($"get_{colName}", methodAttrs, colType, Type.EmptyTypes);
                ILGenerator getterIL = getter.GetILGenerator();
                getterIL.Emit(OpCodes.Ldarg_0);
                getterIL.Emit(OpCodes.Ldfld, field);
                getterIL.Emit(OpCodes.Ret);
                propBuilder.SetGetMethod(getter);

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

    public static class DatabaseHelper
    {
        public static List<(string Schema, string Table)> GetAllTablesWithSchema(string connectionString)
        {
            var list = new List<(string, string)>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add((reader.GetString(0), reader.GetString(1)));
                        }
                    }
                }
            }
            return list;
        }

        public static Dictionary<string, Dictionary<string, Type>> GetAllTableSchemas(string connectionString)
        {
            var result = new Dictionary<string, Dictionary<string, Type>>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string schema = reader.GetString(0);
                            string table = reader.GetString(1);
                            string column = reader.GetString(2);
                            string sqlType = reader.GetString(3);
                            Type dotnetType = ConvertSqlTypeToDotNet(sqlType);
                            string key = $"{schema}.{table}";
                            if (!result.ContainsKey(key))
                                result[key] = new Dictionary<string, Type>();
                            result[key][column] = dotnetType;
                        }
                    }
                }
            }
            return result;
        }

        public static Dictionary<string, string> GetAllPrimaryKeys(string connectionString)
        {
            var result = new Dictionary<string, string>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("SELECT TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string schema = reader.GetString(0);
                            string table = reader.GetString(1);
                            string column = reader.GetString(2);
                            string key = $"{schema}.{table}";
                            if (!result.ContainsKey(key))
                                result[key] = column;
                        }
                    }
                }
            }
            return result;
        }

        private static Type ConvertSqlTypeToDotNet(string sqlType)
        {
            // SQL tipini küçük harfe çeviriyoruz
            return sqlType.ToLower() switch
            {
                "int" => typeof(int),
                "bigint" => typeof(long),
                "smallint" => typeof(short),
                "nvarchar" => typeof(string),
                "varchar" => typeof(string),
                "char" => typeof(string),
                "uniqueidentifier" => typeof(Guid),
                "datetime" => typeof(DateTime),
                "date" => typeof(DateTime),
                "bit" => typeof(bool),
                "decimal" => typeof(decimal),
                "numeric" => typeof(decimal),
                "float" => typeof(double),
                "real" => typeof(float),
                _ => typeof(string)
            };
        }

        public static Dictionary<string, Type> GetTableSchema(string connectionString, string schema, string tableName)
        {
            var allSchemas = GetAllTableSchemas(connectionString);
            string key = $"{schema}.{tableName}";
            if (allSchemas.TryGetValue(key, out Dictionary<string, Type> cols))
                return cols;
            return new Dictionary<string, Type>();
        }

        public static KeyValuePair<string, Type> GetPrimaryKeyColumn(string connectionString, string schema, string tableName, Dictionary<string, Type> columns)
        {
            var allPK = GetAllPrimaryKeys(connectionString);
            string key = $"{schema}.{tableName}";
            if (allPK.TryGetValue(key, out string pkColumn))
            {
                if (columns.TryGetValue(pkColumn, out Type pkType))
                    return new KeyValuePair<string, Type>(pkColumn, pkType);
            }
            return new KeyValuePair<string, Type>("Id", typeof(long));
        }
    }

    public static class RepositoryHelper
    {
        public static void AddRepositories(IServiceCollection services, Dictionary<string, Type> entities)
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

    public class DynamicLinqmsControllerFeatureProvider : IApplicationFeatureProvider<ControllerFeature>
    {
        private readonly Dictionary<string, Type> _entities;

        public DynamicLinqmsControllerFeatureProvider(Dictionary<string, Type> entities)
        {
            _entities = entities;
        }

        public void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            foreach (var kvp in _entities)
            {
                string fullKey = kvp.Key; // "schema.table"
                var entityType = kvp.Value;
                var idType = entityType.BaseType?.GetGenericArguments()[0] ?? typeof(long);
                var controllerType = typeof(LinqmsController<,>).MakeGenericType(entityType, idType);

                // Parse schema and table from key
                var partsArr = fullKey.Split('.');
                string schema = partsArr[0];
                string table = partsArr[1];

                // Eğer schema "dbo" ise, controller adı sadece tablo adını, değilse "schema_tablenameController"
                string controllerName = (schema.ToLower() == "dbo" ? table : schema + "_" + table) + "Controller";

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

        private TypeBuilder CreateTypeBuilder(string controllerName)
        {
            var asmName = new AssemblyName("DynamicControllersAssembly");
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            var modBuilder = asmBuilder.DefineDynamicModule("MainModule");
            return modBuilder.DefineType(controllerName, TypeAttributes.Public | TypeAttributes.Class, typeof(ControllerBase));
        }
    }
}

