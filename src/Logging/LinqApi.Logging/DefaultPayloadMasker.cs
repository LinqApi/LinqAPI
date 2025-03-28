namespace LinqApi.Logging
{
    /// <summary>
    /// Represents the default implementation of the <see cref="ILinqPayloadMasker"/> interface.
    /// This implementation performs no masking and returns the original payloads as-is.
    /// </summary>
    public class DefaultPayloadMasker : ILinqPayloadMasker
    {
        /// <summary>
        /// DOESNT Mask. => please implement your own payload masker!
        /// </summary>
        /// <param name="request">The request payload to mask.</param>
        /// <returns>The unmodified request payload.</returns>
        public string MaskRequest(string request)
            => request;

        /// <summary>
        /// DOESNT Mask. => please implement your own payload masker!
        /// </summary>
        /// <param name="response">The response payload to mask.</param>
        /// <returns>The unmodified response payload.</returns>
        public string MaskResponse(string response)
            => response;
    }
}