using Newtonsoft.Json;

namespace LinqApi.Core
{
    public class DefaultPayloadMasker : ILinqPayloadMasker
    {
        public string MaskRequest(object request)
            => JsonConvert.SerializeObject(request);
        public string MaskResponse(object response)
            => JsonConvert.SerializeObject(response);
    }


}

