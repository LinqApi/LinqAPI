namespace LinqApi.Core
{
    /// <summary>
    /// Simple user context. In production, inject a proper IUserContext.
    /// </summary>
    public class LinqAnonymousUserContext : IUserContext<string>
    {
        public LinqAnonymousUserContext()
        {
        }

        public string Id => "Anonymous";

        string IUserContext<string>.Id { get => Id; set => throw new NotImplementedException(); }
    }

    public interface IUserContext<T>
    {
        public T Id { get; set; }
    }
}