namespace LinqApi.Tools.Helpers
{
    public static class SchemaNormalizationHelper
    {
        public static string Normalize(string schema, string tableName)
        {
            return schema.Equals("dbo", StringComparison.OrdinalIgnoreCase) ? tableName : $"{schema}_{tableName}";
        }
    }

    public static class SqlTypeMapper
    {
        public static Type Map(string sqlType)
        {
            return sqlType.ToLower() switch
            {
                "int" => typeof(int),
                "bigint" => typeof(long),
                "nvarchar" => typeof(string),
                // diğer tip dönüşümleri...
                _ => typeof(string)
            };
        }

    }
}
