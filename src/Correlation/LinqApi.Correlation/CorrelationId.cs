using LinqApi.Epoch;

namespace LinqApi.Correlation
{
    /// <summary>
    /// Represents a correlation identifier.
    /// </summary>
    public readonly struct CorrelationId
    {
        public Guid Value { get; }
        public byte EnvironmentCode => Value.ToByteArray()[0];
        public byte SourceTypeCode => Value.ToByteArray()[1];
        public long Epoch => LinqEpoch.Now();

        public CorrelationId(Guid value) => Value = value;

        public static CorrelationId Create(byte env, byte sourceType)
        {
            var guidBytes = Guid.NewGuid().ToByteArray();
            guidBytes[0] = env;
            guidBytes[1] = sourceType;

            long epoch = LinqEpoch.Now(); // Static epoch provider çağrısı
            var epochBytes = BitConverter.GetBytes(epoch).Take(6).ToArray();
            Array.Copy(epochBytes, 0, guidBytes, 2, 6); // 6-byte epoch

            return new CorrelationId(new Guid(guidBytes));
        }

        public override string ToString() => Value.ToString();
        public static implicit operator Guid(CorrelationId cid) => cid.Value;
        public static implicit operator CorrelationId(Guid gid) => new(gid);
    }


}

