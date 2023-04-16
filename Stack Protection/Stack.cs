using dnlib.DotNet.Emit;
using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protector.Protections
{
    internal class Stack
    {
        public static void Execute(ModuleDef mod)
        {
            foreach (var type in mod.Types)
            {
                foreach (var meth in type.Methods)
                {
                    if (!meth.HasBody)
                    {
                        break;
                    }

                    var body = meth.Body;
                    var target = body.Instructions[0];
                    var item = Instruction.Create(OpCodes.Br_S, target);
                    var instruction3 = Instruction.Create(OpCodes.Pop);
                    var random = new Random();
                    var instruction4 = GetRandomInstruction(random);

                    body.Instructions.Insert(0, instruction4);
                    body.Instructions.Insert(1, instruction3);
                    body.Instructions.Insert(2, item);

                    if (body.ExceptionHandlers != null)
                    {
                        foreach (var handler in body.ExceptionHandlers)
                        {
                            if (handler.TryStart == target)
                            {
                                handler.TryStart = item;
                            }
                            else if (handler.HandlerStart == target)
                            {
                                handler.HandlerStart = item;
                            }
                            else if (handler.FilterStart == target)
                            {
                                handler.FilterStart = item;
                            }
                        }
                    }
                }
            }
        }

        private static Instruction GetRandomInstruction(Random random)
        {
            var opcode = random.Next(0, 5);

            switch (opcode)
            {
                case 0:
                    return Instruction.Create(OpCodes.Ldnull);
                case 1:
                    return Instruction.Create(OpCodes.Ldc_I4_0);
                case 2:
                    return Instruction.Create(OpCodes.Ldstr, "Isolator");
                case 3:
                    return Instruction.Create(OpCodes.Ldc_I8, (uint)random.Next());
                default:
                    return Instruction.Create(OpCodes.Ldc_I8, (long)random.Next());
            }
        }

    }
}
