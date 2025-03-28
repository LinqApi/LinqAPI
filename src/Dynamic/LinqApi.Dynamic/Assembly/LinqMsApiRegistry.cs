using LinqApi.Dynamic.Configuration;

namespace LinqApi.Dynamic.Assembly
{
    public static class LinqMsApiRegistry
    {
        // Key: Area name, Value: Konfigürasyon bilgileri
        public static Dictionary<string, LinqMsApiConfiguration> Configurations { get; } = new Dictionary<string, LinqMsApiConfiguration>();
    }

}
