using LinqApi.Tools.Abstractions;

namespace LinqApi.Generators.SqlServer
{
    public class SqlServerTypeMapper : ISqlTypeMapper
    {
        public Type Map(string sqlType)
        {
            return sqlType.ToLowerInvariant() switch
            {
                "int" => typeof(int),
                "bigint" => typeof(long),
                "smallint" => typeof(short),
                "tinyint" => typeof(byte),
                "bit" => typeof(bool),
                "decimal" => typeof(decimal),
                "numeric" => typeof(decimal),
                "money" => typeof(decimal),
                "float" => typeof(double),
                "real" => typeof(float),
                "datetime" => typeof(DateTime),
                "datetime2" => typeof(DateTime),
                "smalldatetime" => typeof(DateTime),
                "date" => typeof(DateTime),
                "time" => typeof(TimeSpan),
                "varchar" => typeof(string),
                "nvarchar" => typeof(string),
                "text" => typeof(string),
                "uniqueidentifier" => typeof(Guid),
                "binary" => typeof(byte[]),
                "varbinary" => typeof(byte[]),
                _ => typeof(string)
            };
        }
    }
}
