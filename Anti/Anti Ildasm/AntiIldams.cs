using dnlib.DotNet;

namespace Satfuscator.Protections
{
    internal class AntiIldasm
    {
        public static void Execute(Context ctx)
        {
            // The code is an implementation of the AntiIldasm protection, which prevents disassembly of .NET assemblies using tools like Ildasm. 
            // This specific method is called when the protection is executed and it iterates over each module in the assembly to add a custom attribute that marks the module as SuppressIldasm

            foreach (ModuleDefMD module in ctx.assemblyDef.Modules)
            {
                TypeRef attrRef = module.CorLibTypes.GetTypeRef("System.Runtime.CompilerServices", "SuppressIldasmAttribute");
                var ctorRef = new MemberRefUser(module, ".ctor", MethodSig.CreateInstance(module.CorLibTypes.Void), attrRef);

                var attr = new CustomAttribute(ctorRef);
                module.CustomAttributes.Add(attr);
            }
        }
    }
}
