using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Protector.Protections
{
    internal class IntV2
    {
        public static void Execute(Context ctx)
        {
            // This class implements an obfuscation technique that replaces all constant int values with a call to Math.Abs
            // followed by a series of Neg instructions.

            int Amount = 0;
            IMethod absMethod = ctx.moduleDef.Import(typeof(Math).GetMethod("Abs", new Type[] { typeof(int) }));
            IMethod minMethod = ctx.moduleDef.Import(typeof(Math).GetMethod("Min", new Type[] { typeof(int), typeof(int) }));

            // Loop through all types and methods in the moduleDef
            foreach (TypeDef type in ctx.moduleDef.Types)
                foreach (MethodDef method in type.Methods)
                {
                    // If the method doesn't have a body, skip it
                    if (!method.HasBody)
                        continue;

                    // Loop through all instructions in the method body
                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                        if (method.Body.Instructions[i] != null && method.Body.Instructions[i].IsLdcI4())
                        {
                            // Get the constant int operand
                            int operand = method.Body.Instructions[i].GetLdcI4Value();

                            // If the operand is less than or equal to 0, skip it
                            if (operand <= 0)
                                continue;

                            // Insert a call to Math.Abs after the ldc.i4 instruction
                            method.Body.Instructions.Insert(i + 1, OpCodes.Call.ToInstruction(absMethod));

                            // Generate a random number of Neg instructions to insert after the Math.Abs call
                            int neg = Next(StringLength(), 8);
                            if (neg % 2 != 0)
                                neg += 1;

                            for (var j = 0; j < neg; j++)
                                method.Body.Instructions.Insert(i + j + 1, Instruction.Create(OpCodes.Neg));

                            // If the operand is less than int.MaxValue, insert a call to Math.Min after the Neg instructions
                            if (operand < int.MaxValue)
                            {
                                method.Body.Instructions.Insert(i + 1, OpCodes.Ldc_I4.ToInstruction(int.MaxValue));
                                method.Body.Instructions.Insert(i + 2, OpCodes.Call.ToInstruction(minMethod));
                            }

                            ++Amount;
                        }
                }
        }

        private static readonly RandomNumberGenerator csp = RandomNumberGenerator.Create();

        public static string String(int size)
        {
            return Encoding.UTF7.GetString(RandomBytes(size))
                .Replace("\0", ".")
                .Replace("\n", ".")
                .Replace("\r", ".");
        }

        public static int Next()
        {
            return BitConverter.ToInt32(RandomBytes(sizeof(int)), 0);
        }

        private static uint RandomUInt()
        {
            return BitConverter.ToUInt32(RandomBytes(sizeof(uint)), 0);
        }

        private static byte[] RandomBytes(int bytes)
        {
            byte[] buffer = new byte[bytes];
            csp.GetBytes(buffer);
            return buffer;
        }

        public static int Next(int maxValue, int minValue = 0)
        {
            if (minValue >= maxValue)
                throw new ArgumentOutOfRangeException(nameof(minValue));

            long diff = (long)maxValue - minValue;
            long upperBound = uint.MaxValue / diff * diff;
            uint ui;
            do { ui = RandomUInt(); } while (ui >= upperBound);
            return (int)(minValue + (ui % diff));
        }

        public static int StringLength()
        {
            return Next(120, 30);
        }
    }
}