using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Linq;

namespace Protector.Protections
{
    internal class ConstMelting
    {
        public static void Execute(Context context)
        {
            foreach (TypeDef type in context.moduleDef.Types.ToArray())
            {
                foreach (MethodDef method in type.Methods.ToArray())
                {
                    ReplaceStringLiterals(method);
                    ReplaceIntLiterals(method);
                }
            }
        }

        private static void ReplaceStringLiterals(MethodDef methodDef)
        {
            if (CanObfuscate(methodDef))
            {
                foreach (Instruction instruction in methodDef.Body.Instructions)
                {
                    if (instruction.OpCode != OpCodes.Ldstr) continue;
                    MethodDef replacementMethod = new MethodDefUser(Renamer.GenerateName(), MethodSig.CreateStatic(methodDef.DeclaringType.Module.CorLibTypes.String), MethodImplAttributes.IL | MethodImplAttributes.Managed, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig) { Body = new CilBody() };
                    replacementMethod.Body.Instructions.Add(new Instruction(OpCodes.Ldstr, instruction.Operand.ToString()));
                    replacementMethod.Body.Instructions.Add(new Instruction(OpCodes.Ret));
                    methodDef.DeclaringType.Methods.Add(replacementMethod);
                    instruction.OpCode = OpCodes.Call;
                    instruction.Operand = replacementMethod;
                }
            }
        }

        private static void ReplaceIntLiterals(MethodDef methodDef)
        {
            if (CanObfuscate(methodDef))
            {
                foreach (Instruction instruction in methodDef.Body.Instructions)
                {
                    if (instruction.OpCode != OpCodes.Ldc_I4) continue;
                    MethodDef replacementMethod = new MethodDefUser(Renamer.GenerateName(), MethodSig.CreateStatic(methodDef.DeclaringType.Module.CorLibTypes.Int32), MethodImplAttributes.IL | MethodImplAttributes.Managed, MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig) { Body = new CilBody() };
                    replacementMethod.Body.Instructions.Add(new Instruction(OpCodes.Ldc_I4, instruction.GetLdcI4Value()));
                    replacementMethod.Body.Instructions.Add(new Instruction(OpCodes.Ret));
                    methodDef.DeclaringType.Methods.Add(replacementMethod);
                    instruction.OpCode = OpCodes.Call;
                    instruction.Operand = replacementMethod;
                }
            }
        }

        public static bool CanObfuscate(MethodDef methodDef)
        {
            if (!methodDef.HasBody)
                return false;
            if (!methodDef.Body.HasInstructions)
                return false;
            if (methodDef.DeclaringType.IsGlobalModuleType)
                return false;
            return true;
        }
    }
}