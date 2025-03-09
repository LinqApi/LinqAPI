using System.Collections.Concurrent;

namespace LinqApi.Model
{
    public class LinqMsApiConfiguration
    {
        public string AreaName { get; set; }
        public string ConnectionString { get; set; }
        public ConcurrentDictionary<string, Type> DynamicEntities { get; set; }
        public ConcurrentDictionary<string, string> PrimaryKeyMappings { get; set; }
        public Dictionary<string, Dictionary<string, ColumnDefinition>> ColumnSchemas { get; set; }
    }

    public static class LinqMsApiRegistry
    {
        // Key: Area name, Value: Konfigürasyon bilgileri
        public static Dictionary<string, LinqMsApiConfiguration> Configurations { get; } = new Dictionary<string, LinqMsApiConfiguration>();
    }

}
