using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Protector.Protections.StringEcrypt
{
    internal class StringEcnryption
    {
        public static void Execute(Context ctx)
        {
            int Amount = 0;
            ModuleDefMD typeModule = ModuleDefMD.Load(typeof(StringDecoder).Module);
            TypeDef typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(typeof(StringDecoder).MetadataToken));
            IEnumerable<IDnlibDef> members = Injection.Inject(typeDef, ctx.moduleDef.GlobalType, ctx.typeDef.Module);
            MethodDef init = (MethodDef)members.Single(method => method.Name == "Decrypt");

            foreach (MethodDef method in ctx.typeDef.Module.GlobalType.Methods)
                if (method.Name.Equals(".ctor"))
                {
                    ctx.typeDef.Module.GlobalType.Remove(method);
                    break;
                }

            foreach (TypeDef type in ctx.typeDef.Module.Types)
                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody)
                        continue;

                    method.Body.SimplifyBranches();

                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                        if (method.Body.Instructions[i] != null && method.Body.Instructions[i].OpCode == OpCodes.Ldstr)
                        {
                            int key = IntV2.Next();
                            object op = method.Body.Instructions[i].Operand;

                            if (op == null)
                                continue;

                            method.Body.Instructions[i].Operand = Encrypt(op.ToString(), key);
                            method.Body.Instructions.Insert(i + 1, OpCodes.Ldc_I4.ToInstruction(IntV2.Next()));
                            method.Body.Instructions.Insert(i + 2, OpCodes.Ldc_I4.ToInstruction(key));
                            method.Body.Instructions.Insert(i + 3, OpCodes.Ldc_I4.ToInstruction(IntV2.Next()));
                            method.Body.Instructions.Insert(i + 4, OpCodes.Ldc_I4.ToInstruction(IntV2.Next()));
                            method.Body.Instructions.Insert(i + 5, OpCodes.Ldc_I4.ToInstruction(IntV2.Next()));
                            method.Body.Instructions.Insert(i + 6, OpCodes.Call.ToInstruction(init));

                            ++Amount;
                        }

                    method.Body.OptimizeBranches();
                }
        }
        public static string Encrypt(string str, int key)
        {
            StringBuilder builder = new StringBuilder();
            foreach (char c in str.ToCharArray())
                builder.Append((char)(c + key));

            return builder.ToString();
        }

        public static class StringDecoder
        {
            public static string Decrypt(string str, int min, int key, int hash, int length, int max)
            {
                if (max > 78787878) ;
                if (length > 485941) ;

                StringBuilder builder = new StringBuilder();
                foreach (char c in str.ToCharArray())
                    builder.Append((char)(c - key));

                if (min < 14141) ;
                if (length < 1548174) ;

                return builder.ToString();
            }
        }


    }
}