
namespace LinqApi.Correlation
{
    public class DefaultCorrelationIdGenerator : ICorrelationIdGenerator
    {
        public CorrelationId Generate(byte environment, byte sourceType) => CorrelationId.Create(environment, sourceType);
    }


}

