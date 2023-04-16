using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Protector.Protections
{
    internal static class IntEncoding
    {
        private static readonly RandomNumberGenerator csp = RandomNumberGenerator.Create();

        public static int Next(int minValue = 0, int maxValue = int.MaxValue)
        {
            if (minValue >= maxValue)
                throw new ArgumentOutOfRangeException(nameof(minValue));

            long diff = (long)maxValue - minValue;
            long upperBound = uint.MaxValue / diff * diff;
            uint ui;
            do { ui = RandomUInt(); } while (ui >= upperBound);
            return (int)(minValue + (ui % diff));
        }

        public static string GenerateRandomString(int length)
        {
            byte[] randomBytes = RandomBytes(length);
            return Encoding.UTF7.GetString(randomBytes)
                .Replace("\0", ".")
                .Replace("\n", ".")
                .Replace("\r", ".");
        }

        public static int GetRandomStringLength()
        {
            return Next(30, 120);
        }

        public static int GetRandomInt32()
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

        public static void Execute(Context context)
        {
            // // This code implements an obfuscation technique called "integer encoding", which is used to make reverse engineering more difficult.
            // It works by replacing integer constants in the code with a series of instructions that encode the same value in a more complex way.
            // This implementation uses a combination of bitwise operations, negation, and randomization to create the encoded instructions.
            // The IntEncoding class contains a set of static methods that are used to generate random values and manipulate them in various ways, and the Execute method applies the encoding transformation to all integer literals in the code.

            IMethod absMethod = context.moduleDef.Import(typeof(Math).GetMethod("Abs", new Type[] { typeof(int) }));
            IMethod minMethod = context.moduleDef.Import(typeof(Math).GetMethod("Min", new Type[] { typeof(int), typeof(int) }));

            foreach (TypeDef type in context.moduleDef.Types)
            {
                if (type.IsGlobalModuleType)
                    continue;

                foreach (MethodDef method in type.Methods)
                {
                    if (!method.HasBody)
                        continue;

                    for (int i = 0; i < method.Body.Instructions.Count; i++)
                    {
                        Instruction instruction = method.Body.Instructions[i];
                        if (!(instruction.Operand is int operand) || operand <= 0)
                            continue;

                        method.Body.Instructions.Insert(i + 1, OpCodes.Call.ToInstruction(absMethod));

                        int neg = Next(8, GetRandomStringLength());
                        if (neg % 2 != 0)
                            neg += 1;

                        for (int j = 0; j < neg; j++)
                            method.Body.Instructions.Insert(i + j + 1, Instruction.Create(OpCodes.Neg));

                        if (operand < int.MaxValue)
                        {
                            method.Body.Instructions.Insert(i + 1, OpCodes.Ldc_I4.ToInstruction(int.MaxValue));
                            method.Body.Instructions.Insert(i + 2, OpCodes.Call.ToInstruction(minMethod));
                        }

                        i += neg + 2;
                    }

                    method.Body.SimplifyBranches();
                }
            }
        }
    }
}