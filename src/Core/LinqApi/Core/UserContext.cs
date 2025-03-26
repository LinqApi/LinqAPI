namespace LinqApi.Core
{
    /// <summary>
    /// Simple user context. In production, inject a proper IUserContext.
    /// </summary>
    public static class UserContext
    {
        public static string CurrentUserId { get; set; } = "Anonymous";
    }


}

