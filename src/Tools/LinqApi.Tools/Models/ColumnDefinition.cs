namespace LinqApi.Tools.Models
{
    public class ColumnDefinition
    {
        public string Name { get; set; }
        public Type DotNetType { get; set; }
        public bool IsNullable { get; set; }
        public int? MaxLength { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public int? NumericPrecision { get; set; }
        public int? NumericScale { get; set; }
        public bool IsPrimaryKey { get; set; }
    }

}
