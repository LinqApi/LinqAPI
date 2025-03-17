namespace LinqApi.Tools.Models
{
    public class DatabaseTable
    {
        public string Schema { get; set; }
        public string Name { get; set; }
        public PrimaryKeyDefinition PrimaryKey { get; set; }
        public List<ColumnDefinition> Columns { get; set; }
        public List<ForeignKeyDefinition> ForeignKeys { get; set; }
    }
}