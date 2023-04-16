using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;

namespace Protector.Protections
{
    internal class RenamerV2
    {
        public static void Execute(Context ctx)
        {
            foreach (TypeDef type in ctx.moduleDef.Types)
            {
                Rename(type);
            }

            foreach (TypeDef type in ctx.moduleDef.Types)
            {
                foreach (MethodDef method in type.Methods)
                {
                    Rename(type);

                    foreach (Instruction instr in method.Body.Instructions)
                    {
                        if (instr.OpCode == OpCodes.Call || instr.OpCode == OpCodes.Callvirt)
                        {
                            MethodDef calledMethod = instr.Operand as MethodDef;
                            if (calledMethod != null && calledMethod.DeclaringType != type)
                            {
                                Rename(calledMethod);
                                instr.Operand = calledMethod;
                            }
                        }
                        else if (instr.OpCode == OpCodes.Ldfld || instr.OpCode == OpCodes.Stfld || instr.OpCode == OpCodes.Ldsfld || instr.OpCode == OpCodes.Stsfld)
                        {
                            FieldDef field = instr.Operand as FieldDef;
                            if (field != null && field.DeclaringType != type)
                            {
                                Rename(field);
                                instr.Operand = field;
                            }
                        }
                    }
                }
            }
        }

        public static readonly Random _random = new Random();

        public static void Rename(TypeDef type)
        {
            if (type.IsGlobalModuleType) return;

            string newName = Renamer.GenerateName();
            type.Namespace = newName;
            type.Name = newName;
        }

        public static void Rename(MethodDef method)
        {
            if (method.IsConstructor || method.IsRuntimeSpecialName) return;

            string newName = Renamer.GenerateName();
            method.Name = newName;
        }

        public static void Rename(FieldDef field)
        {
            if (field.IsLiteral) return;

            string newName = Renamer.GenerateName();
            field.Name = newName;
        }
    }
}