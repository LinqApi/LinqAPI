namespace LinqApi.Tools.Models
{
    public class ForeignKeyDefinition
    {
        public string ColumnName { get; set; }
        public string ReferencedSchema { get; set; }
        public string ReferencedTable { get; set; }
        public string ReferencedColumn { get; set; }
    }

}
