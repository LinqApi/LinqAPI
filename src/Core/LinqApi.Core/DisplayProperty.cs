namespace LinqApi.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class DisplayPropertyAttribute : Attribute
    {
        public string[] Properties { get; }

        public DisplayPropertyAttribute(params string[] properties)
        {
            Properties = properties;
        }
    }
}
