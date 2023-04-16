using dnlib.DotNet.Emit;
using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Protector.Protections.Injections
{
    internal class MethodHider
    {
        public static void Execute(Context ctx)
        {
            ModuleDef moduleDef = ctx.moduleDef;
            ModuleDefMD moduleDefMD = ModuleDefMD.Load(typeof(AntiDump).Module);
            TypeDef typeDef = moduleDefMD.ResolveTypeDef(MDToken.ToRID(typeof(MethodHider).MetadataToken));
            IEnumerable<IDnlibDef> source = InjectHelper.Inject(typeDef, moduleDef.GlobalType, moduleDef);
            MethodDef method2 = (MethodDef)source.Single((IDnlibDef method) => method.Name == "MethodHiderInj");
            foreach (TypeDef typeDef2 in moduleDef.GetTypes())
            {
                bool isGlobalModuleType = typeDef2.IsGlobalModuleType;
                if (!isGlobalModuleType)
                {
                    foreach (MethodDef methodDef in typeDef2.Methods)
                    {
                        bool flag = !methodDef.HasBody || !methodDef.Body.HasInstructions;
                        if (!flag)
                        {
                            int i = 0;
                            while (i < methodDef.Body.Instructions.Count)
                            {
                                bool flag2 = methodDef.Body.Instructions[i].OpCode == OpCodes.Call;
                                if (flag2)
                                {
                                    bool flag3 = methodDef.Body.Instructions[i].Operand is MethodDef;
                                    if (flag3)
                                    {
                                        try
                                        {
                                            bool flag4 = methodDef.Name != "CopyAll";
                                            if (!flag4)
                                            {
                                                MethodDef methodDef2 = (MethodDef)methodDef.Body.Instructions[i].Operand;
                                                bool flag5 = methodDef2.Parameters.Count == 0;
                                                if (!flag5)
                                                {
                                                    bool flag6 = methodDef2.DeclaringType == moduleDef.Import(typeof(void));
                                                    if (!flag6)
                                                    {
                                                        bool isConstructor = methodDef2.IsConstructor;
                                                        if (!isConstructor)
                                                        {
                                                            bool flag7 = !methodDef2.IsStatic;
                                                            if (!flag7)
                                                            {
                                                                int num = 1;
                                                                Local local = new Local(moduleDef.ImportAsTypeSig(typeof(object[])));
                                                                methodDef.Body.Variables.Add(local);
                                                                methodDef.Body.Instructions.Insert(i + num, OpCodes.Ldc_I4.ToInstruction(methodDef2.Parameters.Count));
                                                                methodDef.Body.Instructions.Insert(i + ++num, OpCodes.Newarr.ToInstruction(moduleDef.CorLibTypes.Object));
                                                                methodDef.Body.Instructions.Insert(i + ++num, OpCodes.Dup.ToInstruction());
                                                                for (int j = 0; j < methodDef2.Parameters.Count; j++)
                                                                {
                                                                    bool flag8 = methodDef.Body.Instructions[i - j - 1].OpCode == OpCodes.Call;
                                                                    if (flag8)
                                                                    {
                                                                        methodDef.Body.Instructions.RemoveAt(i + num);
                                                                        methodDef.Body.Instructions.RemoveAt(i + --num);
                                                                        methodDef.Body.Instructions.RemoveAt(i + --num);
                                                                        break;
                                                                    }
                                                                    bool flag9 = methodDef.Body.Instructions[i - j - 1].OpCode == OpCodes.Nop;
                                                                    if (!flag9)
                                                                    {
                                                                        methodDef.Body.Instructions.Insert(i + ++num, OpCodes.Ldc_I4.ToInstruction(j));
                                                                        methodDef.Body.Instructions.Insert(i + ++num, methodDef.Body.Instructions[i - j - 1]);
                                                                        methodDef.Body.Instructions.Insert(i + ++num, OpCodes.Stelem_Ref.ToInstruction());
                                                                        bool flag10 = methodDef2.Parameters.Count - 1 != j;
                                                                        if (flag10)
                                                                        {
                                                                            methodDef.Body.Instructions.Insert(i + ++num, OpCodes.Dup.ToInstruction());
                                                                        }
                                                                    }
                                                                }
                                                                methodDef.Body.Instructions.Insert(i + ++num, OpCodes.Ldc_I4.ToInstruction(methodDef2.MDToken.ToInt32()));
                                                                methodDef.Body.Instructions.Insert(i + ++num, OpCodes.Call.ToInstruction(method2));
                                                                i += num + 1;
                                                                break;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        catch
                                        {
                                        }
                                    }
                                }
                            IL_4A0:
                                i++;
                                continue;
                                goto IL_4A0;
                            }
                        }
                    }
                }
            }
        }
        public static void MethodHiderInj(object[] parameters, int token)
        {
            parameters.Reverse<object>();
            Assembly executingAssembly = Assembly.GetExecutingAssembly();
            Assembly callingAssembly = Assembly.GetCallingAssembly();
            bool flag = executingAssembly == callingAssembly;
            if (flag)
            {
                Module manifestModule = executingAssembly.ManifestModule;
                MethodInfo methodInfo = (MethodInfo)manifestModule.ResolveMethod(token);
                methodInfo.Invoke(null, parameters);
            }
        }
    }
}
