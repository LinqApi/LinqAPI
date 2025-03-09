namespace LinqApi.Model
{
    public class ColumnDefinition
    {
        public Type DotNetType { get; set; }
        public bool IsNullable { get; set; }
        public int? MaxLength { get; set; }
        public int? NumericPrecision { get; set; }
        public int? NumericScale { get; set; }
    }

}
