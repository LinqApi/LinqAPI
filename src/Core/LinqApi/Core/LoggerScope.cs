namespace LinqApi.Core
{
    /// <summary>
    /// Simple disposable scope helper.
    /// </summary>
    public class LoggerScope : IDisposable
    {
        private readonly Action _onDispose;
        public LoggerScope(Action onDispose) => _onDispose = onDispose;
        public void Dispose() => _onDispose();
    }


}

