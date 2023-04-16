using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;

namespace Protector.Protections
{
    internal class SufConfusionProtection
    {
        public static void Execute(Context context)
        {
            foreach (var type in context.ModuleDef.Types)
            {
                foreach (var method in type.Methods)
                {
                    if (!method.HasBody) continue;

                    var body = method.Body;
                    var target = body.Instructions[0];

                    // Create new instructions to replace the original first instruction
                    var newItem = Instruction.Create(OpCodes.Br_S, target);
                    var popItem = Instruction.Create(OpCodes.Pop);
                    var random = new Random();
                    Instruction newTarget;
                    switch (random.Next(0, 5))
                    {
                        case 0:
                            newTarget = Instruction.Create(OpCodes.Ldnull);
                            break;

                        case 1:
                            newTarget = Instruction.Create(OpCodes.Ldc_I4_0);
                            break;

                        case 2:
                            newTarget = Instruction.Create(OpCodes.Ldstr, "Isolator");
                            break;

                        case 3:
                            newTarget = Instruction.Create(OpCodes.Ldc_I8, (uint)random.Next());
                            break;

                        default:
                            newTarget = Instruction.Create(OpCodes.Ldc_I8, (long)random.Next());
                            break;
                    }

                    // Insert the new instructions at the beginning of the method
                    body.Instructions.Insert(0, newTarget);
                    body.Instructions.Insert(1, popItem);
                    body.Instructions.Insert(2, newItem);

                    // Update any exception handlers that target the original first instruction
                    foreach (var handler in body.ExceptionHandlers)
                    {
                        if (handler.TryStart == target)
                            handler.TryStart = newItem;

                        if (handler.HandlerStart == target)
                            handler.HandlerStart = newItem;

                        if (handler.FilterStart == target)
                            handler.FilterStart = newItem;
                    }
                }
            }
        }
    }
}
