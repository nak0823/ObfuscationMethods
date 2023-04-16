using dnlib.DotNet.Emit;
using dnlib.DotNet;
using System;

namespace Protector.Protections
{
    internal class IntProxy
    {
        // Executes the IntProxy protection on the specified module
        public static void Execute(ModuleDef module)
        {
            // Loop through each type in the module
            foreach (var type in module.GetTypes())
            {
                // Skip the global module type
                if (type.IsGlobalModuleType) continue;

                // Loop through each method in the type
                foreach (var meth in type.Methods)
                {
                    // Skip methods without a body
                    if (!meth.HasBody) continue;

                    // Loop through each instruction in the method's body
                    for (var i = 0; i < meth.Body.Instructions.Count; i++)
                    {
                        // Check if the instruction is loading an integer constant
                        if (meth.Body.Instructions[i].IsLdcI4())
                        {
                            // Create a new method to replace the instruction
                            var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                            var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
                            var meth1 = new MethodDefUser(Renamer.GenerateName(),
                                        MethodSig.CreateStatic(module.CorLibTypes.Int32),
                                        methImplFlags, methFlags);
                            module.GlobalType.Methods.Add(meth1);
                            meth1.Body = new CilBody();
                            meth1.Body.Variables.Add(new Local(module.CorLibTypes.Int32));
                            meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_I4, meth.Body.Instructions[i].GetLdcI4Value()));
                            meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

                            // Replace the instruction with a call to the new method
                            meth.Body.Instructions[i].OpCode = OpCodes.Call;
                            meth.Body.Instructions[i].Operand = meth1;
                        }
                        // Check if the instruction is loading a float constant
                        else if (meth.Body.Instructions[i].OpCode == OpCodes.Ldc_R4)
                        {
                            // Create a new method to replace the instruction
                            var methImplFlags = MethodImplAttributes.IL | MethodImplAttributes.Managed;
                            var methFlags = MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig | MethodAttributes.ReuseSlot;
                            var meth1 = new MethodDefUser(Renamer.GenerateName(),
                                        MethodSig.CreateStatic(module.CorLibTypes.Double),
                                        methImplFlags, methFlags);
                            module.GlobalType.Methods.Add(meth1);
                            meth1.Body = new CilBody();
                            meth1.Body.Variables.Add(new Local(module.CorLibTypes.Double));
                            meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ldc_R4, (float)meth.Body.Instructions[i].Operand));
                            meth1.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

                            // Replace the instruction with a call to the new method
                            meth.Body.Instructions[i].OpCode = OpCodes.Call;
                            meth.Body.Instructions[i].Operand = meth1;
                        }
                    }
                }
            }
        }
    }
}
