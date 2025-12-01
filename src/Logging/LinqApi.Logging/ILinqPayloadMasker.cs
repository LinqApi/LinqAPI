namespace LinqApi.Core
{
    /// <summary>
    /// Interface to mask sensitive request/response payloads.
    /// </summary>
    public interface ILinqPayloadMasker
    {
        string MaskRequest(string request);
        string MaskResponse(string response);
    }


}

