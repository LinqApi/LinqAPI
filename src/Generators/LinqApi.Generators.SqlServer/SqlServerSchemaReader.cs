using LinqApi.Tools.Abstractions;
using LinqApi.Tools.Models;
using Microsoft.Data.SqlClient;

namespace LinqApi.Generators.SqlServer
{
    public class SqlServerSchemaReader : IDatabaseSchemaReader
    {
        private readonly ISqlTypeMapper _typeMapper = new SqlServerTypeMapper();

        public IEnumerable<DatabaseTable> ReadTables(string connectionString)
        {
            using var conn = new SqlConnection(connectionString);
            conn.Open();

            string sql = @"
            SELECT 
                sch.name AS SchemaName,
                tbl.name AS TableName,
                col.name AS ColumnName,
                typ.name AS DataType,
                col.is_nullable AS IsNullable,
                col.max_length AS MaxLength,
                col.precision AS NumericPrecision,
                col.scale AS NumericScale,
                CASE WHEN pk.column_id IS NOT NULL THEN 1 ELSE 0 END AS IsPrimaryKey,
                ref_sch.name AS ReferencedSchema,
                ref_tbl.name AS ReferencedTable,
                ref_col.name AS ReferencedColumn
            FROM sys.tables tbl
            INNER JOIN sys.schemas sch ON tbl.schema_id = sch.schema_id
            INNER JOIN sys.columns col ON col.object_id = tbl.object_id
            INNER JOIN sys.types typ ON col.user_type_id = typ.user_type_id
            LEFT JOIN (
                SELECT ic.object_id, ic.column_id
                FROM sys.index_columns ic
                INNER JOIN sys.indexes ix ON ic.object_id = ix.object_id AND ic.index_id = ix.index_id
                WHERE ix.is_primary_key = 1
            ) pk ON pk.object_id = col.object_id AND pk.column_id = col.column_id
            LEFT JOIN sys.foreign_key_columns fk ON fk.parent_object_id = tbl.object_id AND fk.parent_column_id = col.column_id
            LEFT JOIN sys.tables ref_tbl ON fk.referenced_object_id = ref_tbl.object_id
            LEFT JOIN sys.schemas ref_sch ON ref_tbl.schema_id = ref_sch.schema_id
            LEFT JOIN sys.columns ref_col ON ref_col.object_id = fk.referenced_object_id AND ref_col.column_id = fk.referenced_column_id
            WHERE sch.name NOT IN ('sys', 'INFORMATION_SCHEMA')";

            var tables = new Dictionary<string, DatabaseTable>();

            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                var schema = reader.GetString(0);
                var tableName = reader.GetString(1);
                var fullName = $"{schema}.{tableName}";

                if (!tables.TryGetValue(fullName, out var table))
                {
                    table = new DatabaseTable
                    {
                        Schema = schema,
                        Name = tableName,
                        Columns = new List<ColumnDefinition>(),
                        ForeignKeys = new List<ForeignKeyDefinition>()
                    };
                    tables[fullName] = table;
                }

                var column = new ColumnDefinition
                {
                    Name = reader.GetString(2),
                    DotNetType = _typeMapper.Map(reader.GetString(3)),
                    IsNullable = reader.GetBoolean(4),
                    MaxLength = reader.IsDBNull(5) ? null : (int?)reader.GetInt16(5),
                    NumericPrecision = reader.IsDBNull(6) ? null : (int?)reader.GetByte(6),
                    NumericScale = reader.IsDBNull(7) ? null : (int?)reader.GetByte(7),
                    IsPrimaryKey = reader.GetInt32(8) == 1
                };

                table.Columns.Add(column);

                if (column.IsPrimaryKey)
                    table.PrimaryKey = new PrimaryKeyDefinition { ColumnName = column.Name, DotNetType = column.DotNetType };

                if (!reader.IsDBNull(9))
                {
                    table.ForeignKeys.Add(new ForeignKeyDefinition
                    {
                        ColumnName = column.Name,
                        ReferencedSchema = reader.GetString(9),
                        ReferencedTable = reader.GetString(10),
                        ReferencedColumn = reader.GetString(11)
                    });
                }
            }

            return tables.Values;
        }
    }
}
