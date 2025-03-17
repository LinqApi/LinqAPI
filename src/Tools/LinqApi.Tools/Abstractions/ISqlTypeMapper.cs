namespace LinqApi.Tools.Abstractions
{
    public interface ISqlTypeMapper
    {
        Type Map(string sqlType);
    }

}
