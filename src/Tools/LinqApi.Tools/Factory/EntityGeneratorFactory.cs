using LinqApi.Tools.Abstractions;
using LinqApi.Tools.Enums;

namespace LinqApi.Tools.Factory
{
    public static class EntityGeneratorFactory
    {
        public static IEntityGenerator GetGenerator(DatabaseType dbType)
        {
            return dbType switch
            {
                DatabaseType.SqlServer => Activator.CreateInstance(Type.GetType("LinqApi.Generators.SqlServer.SqlServerEntityGenerator, LinqApi.Generators.SqlServer")) as IEntityGenerator,
                DatabaseType.MySql => Activator.CreateInstance(Type.GetType("LinqApi.Generators.MySql.MySqlEntityGenerator, LinqApi.Generators.MySql")) as IEntityGenerator,
                DatabaseType.PostgreSql => Activator.CreateInstance(Type.GetType("LinqApi.Generators.PostgreSql.PostgreSqlEntityGenerator, LinqApi.Generators.PostgreSql")) as IEntityGenerator,
                DatabaseType.Sqlite => Activator.CreateInstance(Type.GetType("LinqApi.Generators.PostgreSql.PostgreSqlEntityGenerator, LinqApi.Generators.Sqlite")) as IEntityGenerator,
                _ => throw new NotSupportedException($"{dbType} desteklenmiyor.")
            };
        }

    }
}
