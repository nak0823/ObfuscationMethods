using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Protector.Protections
{
    internal class Junk
    {
        public static Random rnd = new Random();

        public static string[] animeNames = new string[] { "Naruto", "Luffy", "Goku", "Vegeta", "Sanji", "Zoro", "Jotaro", "Dio", "Gon", "Killua",
                                         "Asuka", "Mikasa", "Rei", "Rem", "Emilia", "Ram", "Kagome", "Sango", "Misa", "Lain",
                                         "Yui", "Rin", "Rikka", "Chitoge", "Onodera", "Aria", "Akane", "Kanade", "Kurisu", "Mayuri",
                                         "Sakura", "Ino", "Kurenai", "Satsuki", "Ryuko", "Mako", "Nico", "Maki", "Kotori", "Chika",
                                         "Taiga", "Mai", "Kanna", "Tohru", "Megumin", "Rem", "Ram", "Hifumi", "Raphiel", "Gabriel"
            };
        public static string RndBullshit(int length)
        {
            StringBuilder sb = new StringBuilder();

    
            string arabicCharacters = "ΑαΒβΓγΔδΕεΖζΗηΘθΙιΚκΛλΜμΝνΞξΟοΠπΡρΣσςΤτΥυΦφΧχΨψΩω";
            for (int i = 1; i < length - 1; i++)
            {
                sb.Append(arabicCharacters[rnd.Next(arabicCharacters.Length)]);
            }

            // Return the generated random string
            return sb.ToString();
        }

        public static void Execute(Context ctx)
        {
            for (int i = 0; i < 100; i++)
            {
                TypeDef typeDef = new TypeDefUser(Renamer.GenerateName(), Renamer.GenerateName());
                typeDef.Attributes = 0;
                ctx.moduleDef.Types.Add(typeDef);

                for (int j = 0; j < 5; j++)
                {
                    MethodDef method = new MethodDefUser(Renamer.GenerateName(), MethodSig.CreateStatic(ctx.moduleDef.CorLibTypes.Void));
                    typeDef.Methods.Add(method);

                    CilBody body = new CilBody();

                    for (int k = 0; k < rnd.Next(5, 15); k++)
                    {
                        switch (rnd.Next(6))
                        {
                            case 0:
                                body.Instructions.Add(OpCodes.Nop.ToInstruction());
                                break;

                            case 1:
                                body.Instructions.Add(OpCodes.Ret.ToInstruction());
                                break;

                            case 2:
                                body.Instructions.Add(OpCodes.Ldc_I4.ToInstruction(rnd.Next(int.MinValue, int.MaxValue)));
                                break;

                            case 3:
                                body.Instructions.Add(OpCodes.Ldstr.ToInstruction(Renamer.GenerateName()));
                                break;

                            case 4:
                                body.Instructions.Add(OpCodes.Ldfld.ToInstruction(new FieldDefUser(Renamer.GenerateName(), new FieldSig(ctx.moduleDef.CorLibTypes.Int32))));
                                break;

                            case 5:
                                body.Instructions.Add(OpCodes.Ldsfld.ToInstruction(new FieldDefUser(Renamer.GenerateName(), new FieldSig(ctx.moduleDef.CorLibTypes.Int32))));
                                break;
                        }
                    }
                    method.Body = body;
                }
            }

            var moduleType = ctx.moduleDef.Types.FirstOrDefault(t => t.IsGlobalModuleType);

            if (moduleType == null)
            {
                throw new Exception("<Module> type not found");
            }

         

            for (int i = 0; i < 10000; i++)
            {
                // Define a void method with the correct signature
                var method = new MethodDefUser(
                    animeNames[rnd.Next(0, 49)] + "_uses_" + RndBullshit(16),
                    MethodSig.CreateStatic(ctx.moduleDef.CorLibTypes.Void),
                    MethodImplAttributes.Managed,
                    MethodAttributes.HideBySig | MethodAttributes.Public | MethodAttributes.Static)
                {
                    Body = new CilBody()
                };

                // Add instructions to perform the sum calculation and trigger an overflow exception
                method.Body.Instructions.Add(OpCodes.Ldfld.ToInstruction(new FieldDefUser(animeNames[rnd.Next(0, 49)], new FieldSig(ctx.moduleDef.CorLibTypes.Int32))));

                // Add the method to the <Module> class
                moduleType.Methods.Add(method);
            }

        }
    }
}