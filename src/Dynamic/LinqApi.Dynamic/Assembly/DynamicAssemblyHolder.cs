using System.Reflection.Emit;
using System.Reflection;

namespace LinqApi.Dynamic.Assembly
{
    public static class DynamicAssemblyHolder
    {
        public static readonly AssemblyBuilder EntityAssemblyBuilder;
        public static readonly ModuleBuilder EntityModuleBuilder;

        public static readonly AssemblyBuilder ControllerAssemblyBuilder;
        public static readonly ModuleBuilder ControllerModuleBuilder;

        static DynamicAssemblyHolder()
        {
            AssemblyName asmName = new AssemblyName("DynamicEntities");
            // RunAndCollect ile toplanabilir assembly oluşturuyoruz
            EntityAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.RunAndCollect);
            EntityModuleBuilder = EntityAssemblyBuilder.DefineDynamicModule("MainModule");

            AssemblyName ctrlAsmName = new AssemblyName("DynamicControllersAssembly");
            ControllerAssemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(ctrlAsmName, AssemblyBuilderAccess.RunAndCollect);
            ControllerModuleBuilder = ControllerAssemblyBuilder.DefineDynamicModule("MainModule");
        }
    }
}
