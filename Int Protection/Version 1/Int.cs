using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;

namespace Protector.Protections
{
    internal class Int
    {
        public static void Execute(Context context)
        {
		// The Int protection is a code obfuscation technique that modifies integer constant values in a .NET assembly to make it harder for reverse engineers to understand the code. 
            // It replaces each integer constant with a series of instructions that perform an XOR operation on a random number and a mask value, and then adds the resulting value to the original constant value. 
		// This makes it much more difficult for a reverse engineer to determine the original constant value and understand the code's functionality. 
		// The protection also adds some additional instructions to make the code more difficult to analyze, such as inserting a nop instruction and simplifying branches.

            foreach (TypeDef type in context.moduleDef.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody) continue;
                    {
                        for (var i = 0; i < method.Body.Instructions.Count; i++)
                        {
                            if (!method.Body.Instructions[i].IsLdcI4()) continue;
                            var RndNumber = new Random(Guid.NewGuid().GetHashCode()).Next();
                            var Mask = new Random(Guid.NewGuid().GetHashCode()).Next();
                            var Num = RndNumber ^ Mask;

                            var nop = OpCodes.Nop.ToInstruction();

                            var local = new Local(method.Module.ImportAsTypeSig(typeof(int)));
                            method.Body.Variables.Add(local);

                            method.Body.Instructions.Insert(i + 1, OpCodes.Stloc.ToInstruction(local));
                            method.Body.Instructions.Insert(i + 2, Instruction.Create(OpCodes.Ldc_I4, method.Body.Instructions[i].GetLdcI4Value() - sizeof(float)));
                            method.Body.Instructions.Insert(i + 3, Instruction.Create(OpCodes.Ldc_I4, RndNumber));
                            method.Body.Instructions.Insert(i + 4, Instruction.Create(OpCodes.Ldc_I4, Num));
                            method.Body.Instructions.Insert(i + 5, Instruction.Create(OpCodes.Xor));
                            method.Body.Instructions.Insert(i + 6, Instruction.Create(OpCodes.Ldc_I4, Mask));
                            method.Body.Instructions.Insert(i + 7, Instruction.Create(OpCodes.Bne_Un, nop));
                            method.Body.Instructions.Insert(i + 8, Instruction.Create(OpCodes.Ldc_I4, 2));
                            method.Body.Instructions.Insert(i + 9, OpCodes.Stloc.ToInstruction(local));
                            method.Body.Instructions.Insert(i + 10, Instruction.Create(OpCodes.Sizeof, method.Module.Import(typeof(float))));
                            method.Body.Instructions.Insert(i + 11, Instruction.Create(OpCodes.Add));
                            method.Body.Instructions.Insert(i + 12, nop);
                            i += 12;
                        }
                        method.Body.SimplifyBranches();
                    }
                }
            }
        }
    }
}