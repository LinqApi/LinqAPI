namespace LinqApi.Model
{
    public class PaginationModel<T>
    {
        public List<T> Items { get; set; } = new();
        public int TotalRecords { get; set; }
    }
}
