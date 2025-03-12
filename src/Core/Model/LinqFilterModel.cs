namespace LinqApi.Model
{
    public class LinqFilterModel
    {
        public string Filter { get; set; } // Örnek: "Age > 30"
        public Pager Pager { get; set; } // Ana veri seti için paging
        public string OrderBy { get; set; } // Örnek: "Name desc"
        public bool Desc { get; set; }
        public string GroupBy { get; set; } // Örnek: "new (DepartmentId)"
        public string Select { get; set; } // Örnek: "new (Name, Age, Salary.Max())"
        public List<IncludeModel> Includes { get; set; } = new(); // Include için yeni yapı
    }

    public class IncludeModel
    {
        public string PropertyName { get; set; } // Include edilecek navigation property
        public Pager? Pager { get; set; } // Include için paging
        public List<ThenIncludeModel> ThenIncludes { get; set; } = new(); // ThenInclude desteği
    }

    public class ThenIncludeModel
    {
        public string ParentProperty { get; set; } // Hangi parent üzerinden ThenInclude olacak
        public List<IncludeModel> ChildIncludes { get; set; } = new(); // ThenInclude içindeki Include desteği
    }
}
