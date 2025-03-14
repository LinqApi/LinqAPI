using System.Collections.Concurrent;

namespace LinqApi.Model
{
    public class DynamicEntities : ConcurrentDictionary<string, Type>
    {
        private readonly int processorCount;

        public DynamicEntities(int processorCount)
        {
            this.processorCount = processorCount;
        }

        private readonly ConcurrentDictionary<string, Type> instances;
        public DynamicEntities(ConcurrentDictionary<string, Type> instances, int processorCount)
        {
            this.processorCount = Environment.ProcessorCount;
            this.instances = instances;
            this.processorCount = processorCount;
        }

        public DynamicEntities()
        {
        }

        public DynamicEntities(int concurrencyLevel, int capacity, IEqualityComparer<string> comparer) : base(concurrencyLevel, capacity, comparer)
        {
        }

        public DynamicEntities(int concurrencyLevel, IEnumerable<KeyValuePair<string, Type>> collection, IEqualityComparer<string> comparer) : base(concurrencyLevel, collection, comparer)
        {
        }
    }
}
