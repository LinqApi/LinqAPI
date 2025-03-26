namespace LinqApi.Core
{
    /// <summary>
    /// Interface to mask sensitive request/response payloads.
    /// </summary>
    public interface ILinqPayloadMasker
    {
        string MaskRequest(object request);
        string MaskResponse(object response);
    }


}

