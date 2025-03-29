internal static class DynamicServiceApiProtectedExtensions
{
     internal static string GetControllerName(string schema, string table)
    {
        string normalized = string.IsNullOrEmpty(schema) ? "dbo" : schema.ToLowerInvariant();
        return $"{normalized}_{table}Controller";
    }

}
