using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Protector.Protections
{
    internal class Mutation
    {
        private static ModuleDefMD _moduleDefMd;
        public static void Execute(ModuleDefMD moduleDefMd)
        {
            _moduleDefMd = moduleDefMd;
            var cryptoRandom = new MutationHelper.CryptoRandom();
            foreach (var typeDef in moduleDefMd.GetTypes())
            {
                var listMethod = new List<MethodDef>(); 
                foreach (var methodDef in typeDef.Methods.Where(x => x.HasBody))
                {
                    var instructions = methodDef.Body.Instructions;
                    for (var i = 0; i < instructions.Count; i++)
                    {
                        if (instructions[i].IsLdcI4() && IsSafe(instructions.ToList(), i))
                        {
                            MethodDef refMethod = null;
                            int operand = instructions[i].GetLdcI4Value();
                            instructions[i].OpCode = OpCodes.Ldc_R8;
                            switch (cryptoRandom.Next(0, 3))
                            {
                                case 0:
                                    refMethod = GenerateRefMethod("Floor");
                                    instructions[i].Operand = Convert.ToDouble(operand + cryptoRandom.NextDouble());
                                    break;
                                case 1:
                                    refMethod = GenerateRefMethod("Sqrt");
                                    instructions[i].Operand = Math.Pow(Convert.ToDouble(operand), 2);
                                    break;
                                case 2:
                                    refMethod = GenerateRefMethod("Round");
                                    instructions[i].Operand = Convert.ToDouble(operand);
                                    break;
                            }

                            instructions.Insert(i + 1, OpCodes.Call.ToInstruction(refMethod));
                            instructions.Insert(i + 2, OpCodes.Conv_I4.ToInstruction());
                            i += 2;
                            listMethod.Add(refMethod);
                        }
                    }

                    methodDef.Body.SimplifyMacros(methodDef.Parameters);
                }

                foreach (var method in listMethod)
                    typeDef.Methods.Add(method);
            }
        }


        private static MethodDef GenerateRefMethod(string methodName)
        {
            var refMethod = new MethodDefUser(
                "_" + Guid.NewGuid().ToString("D").ToUpper().Substring(2, 5),
                MethodSig.CreateStatic(_moduleDefMd.ImportAsTypeSig(typeof(double))),
                MethodAttributes.Private | MethodAttributes.Static | MethodAttributes.HideBySig)
            {
                Signature = new MethodSig
                {
                    Params = { _moduleDefMd.ImportAsTypeSig(typeof(double)) },
                    RetType = _moduleDefMd.ImportAsTypeSig(typeof(double))
                }
            };

            var cil = new CilBody
            {
                Instructions =
                {
                    OpCodes.Ldarg_0.ToInstruction(),
                    OpCodes.Call.ToInstruction(GetMethod(typeof(Math),
                        methodName, new[] {typeof(double)})),
                    OpCodes.Stloc_0.ToInstruction(),
                    OpCodes.Ldloc_0.ToInstruction(),
                    OpCodes.Ret.ToInstruction()
                }
            };
            refMethod.Body = cil;
            refMethod.Body.Variables.Add(new Local(_moduleDefMd.ImportAsTypeSig(typeof(double))));
            return refMethod.ResolveMethodDef();
        }

        private static bool IsSafe(List<Instruction> instructions, int i)
        {
            if (new[] { -2, -1, 0, 1, 2 }.Contains(instructions[i].GetLdcI4Value())) // Skipping min values
                return false;
            return true;
        }

        private static IMethod GetMethod(Type type, string methodName, Type[] types)
        {
            return _moduleDefMd.Import(type.GetMethod(methodName, types));
        }

    }
}