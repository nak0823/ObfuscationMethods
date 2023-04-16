using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;

namespace Protector.Protections
{
    internal class InvalidOpcodes
    {
        public static void Execute(ModuleDef module)
        {
            foreach (TypeDef typeDef in module.GetTypes())
            {
                foreach (MethodDef methodDef in typeDef.Methods)
                {
                    if (methodDef.HasBody || methodDef.Body.HasInstructions)
                    {
                        methodDef.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Box, methodDef.Module.Import(typeof(Math))));
                    }
                }
            }
        }
    }
}