using LinqApi.Model;
using System.Collections.Concurrent;

namespace LinqApi.Dynamic.Configuration
{
    public class LinqMsApiConfiguration
    {
        public string AreaName { get; set; }
        public string ConnectionString { get; set; }
        public ConcurrentDictionary<string, Type> DynamicEntities { get; set; }
        public ConcurrentDictionary<string, string> PrimaryKeyMappings { get; set; }
        public Dictionary<string, Dictionary<string, ColumnDefinition>> ColumnSchemas { get; set; }
    }
}
