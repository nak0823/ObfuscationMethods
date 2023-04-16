using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Protector.Protections
{
    internal class AntiDnspy
    {
        public static void Execute(Context ctx)
        {
            // This code implements an obfuscation technique called "AntiDnspy" that adds a large number of NOP (no operation) instructions to the beginning of each method's body in the target assembly.
            // This makes it more difficult for tools like dnSpy to decompile the code, as the extra NOP instructions will need to be removed before the decompiled code can be properly analyzed.
            // The code loops through all the types and methods in the target assembly and adds 33333 NOP instructions to the start of each method's body.

            foreach (TypeDef type in ctx.assemblyDef.ManifestModule.Types)
            {
                foreach (MethodDef method in type.Methods)
                {
                    if (method.Body == null) continue;
                    for (int x = 0; x < 33333; x++)
                    {
                        method.Body.Instructions.Insert(x, new Instruction(OpCodes.Nop));
                    }
                }
            }
        }
    }
}