namespace LinqApi.Model
{
    public class LinqFilterModel
    {
        public string Filter { get; set; }
        public Pager Pager { get; set; }
        public List<string> Includes { get; set; }
        public string Orderby { get; set; }
        public bool Desc { get; set; }
    }
}
