using Microsoft.AspNetCore.Mvc.ApplicationParts;
using System.Reflection;

namespace LinqApi.Razor
{
    public static class MvcHelpers
    {
        public static void CreateViews(ApplicationPartManager apm)
        {
            // RCL (Razor Class Library) assembly'sini de ApplicationPart olarak ekleyin.
            // Örneğin, RCL içerisindeki herhangi bir tipin Assembly'sini kullanabilirsiniz:
            var razorAssembly = typeof(MvcHelpers).GetTypeInfo().Assembly;
            apm.ApplicationParts.Add(new AssemblyPart(razorAssembly));
        }
    }


}
