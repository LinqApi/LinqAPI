using LinqApi.Model;
using Microsoft.Data.SqlClient;

namespace LinqApi.Helpers
{
    public static class DatabaseHelper
    {

        public static List<(string Schema, string Table)> GetAllTablesWithSchema(string connectionString)
        {
            var list = new List<(string, string)>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // Sistem tablolarını hariç tutmak için TABLE_SCHEMA filtrelemesi ekliyoruz.
                using (var cmd = new SqlCommand(
                    @"SELECT TABLE_SCHEMA, TABLE_NAME 
          FROM INFORMATION_SCHEMA.TABLES 
          WHERE TABLE_TYPE = 'BASE TABLE'
          AND TABLE_SCHEMA NOT IN ('sys', 'INFORMATION_SCHEMA')", conn))
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
        public static Dictionary<string, Dictionary<string, ColumnDefinition>> GetAllTableSchemas(string connectionString)
        {
            var result = new Dictionary<string, Dictionary<string, ColumnDefinition>>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE bilgilerini alıyoruz.
                using (var cmd = new SqlCommand(
                    @"SELECT TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION, NUMERIC_SCALE
              FROM INFORMATION_SCHEMA.COLUMNS", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string schema = reader.GetString(0);
                            string table = reader.GetString(1);
                            string column = reader.GetString(2);
                            string sqlType = reader.GetString(3);
                            string isNullableStr = reader.GetString(4);
                            bool isNullable = isNullableStr.Equals("YES", StringComparison.OrdinalIgnoreCase);

                            int? maxLength = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5);
                            int? precision = reader.IsDBNull(6) ? (int?)null : Convert.ToInt32(reader.GetValue(6));
                            int? scale = reader.IsDBNull(7) ? (int?)null : Convert.ToInt32(reader.GetValue(7));

                            Type dotnetType = ConvertSqlTypeToDotNet(sqlType);
                            if (dotnetType == null)
                                continue;
                            if (isNullable && dotnetType.IsValueType)
                                dotnetType = typeof(Nullable<>).MakeGenericType(dotnetType);

                            var colDef = new ColumnDefinition
                            {
                                DotNetType = dotnetType,
                                IsNullable = isNullable,
                                MaxLength = maxLength,
                                NumericPrecision = precision,
                                NumericScale = scale
                            };

                            string key = $"{schema}.{table}";
                            if (!result.ContainsKey(key))
                                result[key] = new Dictionary<string, ColumnDefinition>();

                            result[key][column] = colDef;
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

        public static Type ConvertSqlTypeToDotNet(string sqlType)
        {
            // SQL tipini küçük harfe çeviriyoruz
            return sqlType.ToLower() switch
            {
                "int" => typeof(int),
                "bigint" => typeof(long),
                "smallint" => typeof(short),
                "tinyint" => typeof(byte),
                "bit" => typeof(bool),
                "decimal" => typeof(decimal),
                "numeric" => typeof(decimal),
                "money" => typeof(decimal),
                "smallmoney" => typeof(decimal),
                "float" => typeof(double),
                "real" => typeof(float),
                "date" => typeof(DateTime),
                "datetime" => typeof(DateTime),
                "datetime2" => typeof(DateTime),
                "smalldatetime" => typeof(DateTime),
                "datetimeoffset" => typeof(DateTimeOffset),
                "time" => typeof(TimeSpan),
                "char" => typeof(string),
                "nchar" => typeof(string),
                "varchar" => typeof(string),
                "nvarchar" => typeof(string),
                "text" => typeof(string),
                "ntext" => typeof(string),
                "xml" => typeof(string),
                "uniqueidentifier" => typeof(Guid),
                "binary" => typeof(byte[]),
                "varbinary" => typeof(byte[]),
                "image" => typeof(byte[]),
                "timestamp" => typeof(byte[]), // rowversion için de kullanılır
                "rowversion" => typeof(byte[]),
                "sql_variant" => typeof(object),
                _ => null
            };
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
}
