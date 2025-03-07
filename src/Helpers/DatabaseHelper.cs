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


        public static Dictionary<string, Dictionary<string, Type>> GetAllTableSchemas(string connectionString)
        {
            var result = new Dictionary<string, Dictionary<string, Type>>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                // IS_NULLABLE kolonunu ekledik
                using (var cmd = new SqlCommand(
                    @"SELECT TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
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
                            string isNullableStr = reader.GetString(4); // "YES" veya "NO"
                            bool isNullable = isNullableStr.Equals("YES", StringComparison.OrdinalIgnoreCase);

                            // SQL tipini .NET tipine çeviriyoruz
                            Type dotnetType = ConvertSqlTypeToDotNet(sqlType);

                            // Eğer değer tipi ve null olabilir ise Nullable hale getiriyoruz
                            if (isNullable && dotnetType.IsValueType)
                            {
                                dotnetType = typeof(Nullable<>).MakeGenericType(dotnetType);
                            }

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

        public static Type ConvertSqlTypeToDotNet(string sqlType)
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
}
