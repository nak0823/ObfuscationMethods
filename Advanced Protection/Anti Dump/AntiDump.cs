using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protector.Protections.Injections
{
    internal class AntiDump
    {
        public static void InjectClass(ModuleDef module)
        {
            ModuleDefMD moduleDefMD = ModuleDefMD.Load(typeof(anti_dump_runtime).Module);
            TypeDef typeDef = moduleDefMD.ResolveTypeDef(MDToken.ToRID(typeof(anti_dump_runtime).MetadataToken));
            IEnumerable<IDnlibDef> source = StringEcrypt.Injection.Inject(typeDef, module.GlobalType, module);
            init = (MethodDef)source.Single((IDnlibDef method) => method.Name == "Initialize");
            foreach (MethodDef methodDef in module.GlobalType.Methods)
            {
                bool flag = methodDef.Name == ".ctor";
                if (flag)
                {
                    module.GlobalType.Remove(methodDef);
                    break;
                }
            }
        }

        public static void Execute(ModuleDef module)
        {
            InjectClass(module);
            MethodDef methodDef = module.GlobalType.FindOrCreateStaticConstructor();
            methodDef.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, init));
        }

        public static MethodDef init;
    }
}
