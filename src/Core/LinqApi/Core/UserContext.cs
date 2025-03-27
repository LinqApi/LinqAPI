namespace LinqApi.Core
{
    /// <summary>
    /// Simple user context. In production, inject a proper IUserContext.
    /// </summary>
    public static class LinqAnonymousUserContext
    {
        public static string CurrentUserId { get; set; } = "Anonymous";
    }

    public interface IUserContext<T>
    {
        public abstract T Id { get; set; }
    }
}