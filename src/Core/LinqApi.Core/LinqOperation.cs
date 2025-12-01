/// <summary>
/// Linq tabanlı generic endpoint'lerin anlamsal operasyonlarını temsil eder.
/// HTTP verb'ten bağımsız, daha stabil bir permission key'idir.
/// </summary>
public enum LinqOperation
{
    ReadSingle,
    ReadPaged,
    ReadMetadata,
    Create,
    Update,
    Delete
}