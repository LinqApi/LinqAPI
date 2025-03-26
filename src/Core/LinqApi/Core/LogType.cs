namespace LinqApi.Core
{
    /// <summary>
    /// Log type enumeration.
    /// </summary>
    public enum LogType
    {
        Incoming = 10,   // For incoming HTTP requests
        Outgoing = 20,   // For outgoing HTTP calls
        MassTransitConsume = 31, //For masstransit calls
        MassTransitPublish = 32, //For masstransit calls
        Database = 40,
        Error = 400,
        Info = 200
    }


}

