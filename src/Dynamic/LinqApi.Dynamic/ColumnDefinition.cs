namespace LinqApi.Model
{
    public class ColumnDefinition
    {
        public Type DotNetType { get; set; }
        public bool IsNullable { get; set; }
        public int? MaxLength { get; set; }
        public int? NumericPrecision { get; set; }
        public int? NumericScale { get; set; }
        // Yeni eklenen foreign key bilgileri:
        public string ReferencedSchema { get; set; }
        public string ReferencedTable { get; set; }
        public string ReferencedColumn { get; set; }
    }

}
