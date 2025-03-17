using LinqApi.Model;
using Microsoft.Data.SqlClient;

namespace LinqApi.Dynamic.Helpers
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
                // Yeni sorgu: foreign key ve primary key bilgilerini de alıyor.
                using (var cmd = new SqlCommand(
                    @"SELECT 
                sch.name AS SchemaName,
                tbl.name AS TableName,
                col.name AS ColumnName,
                typ.name AS DataType,
                col.is_nullable AS IsNullable,
                col.max_length AS MaxLength,
                col.precision AS NumericPrecision,
                col.scale AS NumericScale,
                pk_col.COLUMN_NAME AS PrimaryKeyColumn,
                refSch.name AS ReferencedSchema,
                refTbl.name AS ReferencedTable,
                refCol.name AS ReferencedColumn
              FROM sys.tables tbl
              INNER JOIN sys.schemas sch ON tbl.schema_id = sch.schema_id
              INNER JOIN sys.columns col ON col.object_id = tbl.object_id
              INNER JOIN sys.types typ ON col.user_type_id = typ.user_type_id
              LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk_col 
                  ON pk_col.TABLE_NAME = tbl.name AND pk_col.TABLE_SCHEMA = sch.name
              LEFT JOIN sys.foreign_key_columns fkcol
                  ON fkcol.parent_object_id = tbl.object_id AND fkcol.parent_column_id = col.column_id
              LEFT JOIN sys.tables refTbl 
                  ON fkcol.referenced_object_id = refTbl.object_id
              LEFT JOIN sys.schemas refSch 
                  ON refTbl.schema_id = refSch.schema_id
              LEFT JOIN sys.columns refCol 
                  ON refCol.object_id = fkcol.referenced_object_id 
                 AND refCol.column_id = fkcol.referenced_column_id", conn))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // Sütun bilgilerini çekiyoruz.
                            string schema = reader.GetString(0); // SchemaName
                            string table = reader.GetString(1); // TableName
                            string column = reader.GetString(2); // ColumnName
                            string sqlType = reader.GetString(3); // DataType
                            bool isNullable = reader.GetBoolean(4);
                            int? maxLength = reader.IsDBNull(5) ? null : reader.GetInt16(5);
                            int? precision = reader.IsDBNull(6) ? null : Convert.ToInt32(reader.GetValue(6));
                            int? scale = reader.IsDBNull(7) ? null : Convert.ToInt32(reader.GetValue(7));

                            // Primary key sütunu bilgisi (gerektiğinde kullanılabilir)
                            string primaryKeyColumn = reader.IsDBNull(8) ? null : reader.GetString(8);

                            // Foreign key bilgileri
                            string referencedSchema = reader.IsDBNull(9) ? null : reader.GetString(9);
                            string referencedTable = reader.IsDBNull(10) ? null : reader.GetString(10);
                            string referencedColumn = reader.IsDBNull(11) ? null : reader.GetString(11);

                            // SQL tipini .NET tipine çeviriyoruz.
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
                                NumericScale = scale,
                                // Foreign key bilgilerini dolduruyoruz:
                                ReferencedSchema = referencedSchema,
                                ReferencedTable = referencedTable,
                                ReferencedColumn = referencedColumn
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
