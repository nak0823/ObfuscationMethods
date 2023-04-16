using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Globalization;
using System.Linq;

namespace Protector.Protections
{
    internal class Cflow
    {
        public static void Execute(ModuleDef module)
        {
            foreach (TypeDef typeDef in module.Types)
            {
                foreach (MethodDef methodDef in typeDef.Methods.ToArray<MethodDef>())
                {
                    if (methodDef.HasBody && methodDef.Body.HasInstructions && !methodDef.Body.HasExceptionHandlers)
                    {
                        for (int i = 0; i < methodDef.Body.Instructions.Count -2; i++)
                        {
                            Instruction t = methodDef.Body.Instructions[i + 1];
                            methodDef.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Ldstr, "Satfuscator"));
                            methodDef.Body.Instructions.Insert(i + 1, Instruction.Create(OpCodes.Br_S, t));
                            i += 2;
                        }
                    }
                }
            }
        }
    }
}