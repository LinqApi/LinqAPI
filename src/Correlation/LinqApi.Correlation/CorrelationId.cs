using LinqApi.Epoch;

namespace LinqApi.Correlation
{
    /// <summary>
    /// Represents a correlation identifier which embeds environment, source type, and timestamp (epoch) information
    /// within a Guid structure. Useful for tracing, logging, and debugging across distributed systems.
    /// </summary>
    public readonly struct CorrelationId
    {
        /// <summary>
        /// The full GUID value of the correlation ID.
        /// </summary>
        public Guid Value { get; }

        /// <summary>
        /// The environment code stored in the first byte of the Guid.
        /// </summary>
        public byte EnvironmentCode => Value.ToByteArray()[0];

        /// <summary>
        /// The source type code stored in the second byte of the Guid.
        /// </summary>
        public byte SourceTypeCode => Value.ToByteArray()[1];

        /// <summary>
        /// Returns the current Unix epoch timestamp. (Note: This does not extract from GUID)
        /// For actual embedded timestamp, use a parser utility to extract epoch from Guid.
        /// </summary>
        public long Epoch => LinqEpoch.Now();

        /// <summary>
        /// Initializes a new instance of the <see cref="CorrelationId"/> struct using a provided GUID.
        /// </summary>
        /// <param name="value">The Guid containing encoded correlation metadata.</param>
        public CorrelationId(Guid value) => Value = value;

        /// <summary>
        /// Creates a new <see cref="CorrelationId"/> embedding the environment, source type, and current timestamp into a GUID.
        /// </summary>
        /// <param name="env">A single byte representing the environment (e.g. 1 = Prod).</param>
        /// <param name="sourceType">A single byte representing the source type (e.g. 2 = HTTP).</param>
        /// <returns>A new <see cref="CorrelationId"/> instance.</returns>
        public static CorrelationId Create(byte env, byte sourceType)
        {
            var guidBytes = Guid.NewGuid().ToByteArray();

            // Inject environment and sourceType into the first two bytes
            guidBytes[0] = env;
            guidBytes[1] = sourceType;

            // Inject 6-byte Unix epoch timestamp into bytes 2-7
            long epoch = LinqEpoch.Now();
            var epochBytes = BitConverter.GetBytes(epoch).Take(6).ToArray();
            Array.Copy(epochBytes, 0, guidBytes, 2, 6);

            return new CorrelationId(new Guid(guidBytes));
        }

        /// <summary>
        /// Returns the string representation of the correlation GUID.
        /// </summary>
        public override string ToString() => Value.ToString();

        /// <summary>
        /// Implicitly converts <see cref="CorrelationId"/> to <see cref="Guid"/>.
        /// </summary>
        public static implicit operator Guid(CorrelationId cid) => cid.Value;

        /// <summary>
        /// Implicitly converts <see cref="Guid"/> to <see cref="CorrelationId"/>.
        /// </summary>
        public static implicit operator CorrelationId(Guid gid) => new(gid);
    }
}