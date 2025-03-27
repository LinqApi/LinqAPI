using LinqApi.Core.Log;

namespace LinqApi.Logging
{
    // Hata logları için soyut model (türetilmiş hata tiplerini destekler)
    public abstract class LinqErrorLog : LinqLogEntity
    {
        public string StackTrace { get; set; }
        // LogType override alt sınıflarda yapılır.
    }
}

