using MassTransit;

namespace LinqApi.MassTransit
{
    public class DelegatingPublishObserver : IPublishObserver
    {
        private readonly Func<IPublishObserver> _resolver;

        public DelegatingPublishObserver(Func<IPublishObserver> resolver)
        {
            _resolver = resolver;
        }

        public Task PrePublish<T>(PublishContext<T> context) where T : class
            => _resolver().PrePublish(context);

        public Task PostPublish<T>(PublishContext<T> context) where T : class
            => _resolver().PostPublish(context);

        public Task PublishFault<T>(PublishContext<T> context, Exception exception) where T : class
            => _resolver().PublishFault(context, exception);
    }

    public class DelegatingConsumeObserver : IConsumeObserver
    {
        private readonly Func<IConsumeObserver> _resolver;

        public DelegatingConsumeObserver(Func<IConsumeObserver> resolver)
        {
            _resolver = resolver;
        }

        public Task PreConsume<T>(ConsumeContext<T> context) where T : class
            => _resolver().PreConsume(context);

        public Task PostConsume<T>(ConsumeContext<T> context) where T : class
            => _resolver().PostConsume(context);

        public Task ConsumeFault<T>(ConsumeContext<T> context, Exception exception) where T : class
            => _resolver().ConsumeFault(context, exception);
    }

}
