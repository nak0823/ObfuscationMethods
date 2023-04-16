using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Satfuscator.Protections
{
    internal class MutationHelper
    {
        public sealed class CryptoRandom : Random, IDisposable
        {
            private RNGCryptoServiceProvider cryptoProvider = new RNGCryptoServiceProvider();
            private byte[] uint32Buffer = new byte[sizeof(uint)];

            /// <summary>
            /// An implementation of System.Random used for cryptographically-strong random number generation.
            /// </summary>
            public CryptoRandom()
            {
            }

            /// <summary>
            /// An implementation of System.Random used for cryptographically-strong random number generation.
            /// </summary>
            public CryptoRandom(int seedIgnored)
            {
            }

            /// <summary>
            /// Returns a nonnegative random number.
            /// </summary>
            /// <returns>
            /// A 32-bit signed integer greater than or equal to zero and less than <see cref="F:System.Int32.MaxValue"/>.
            /// </returns>
            public override int Next()
            {
                cryptoProvider.GetBytes(uint32Buffer);
                return BitConverter.ToInt32(uint32Buffer, 0) & 0x7FFFFFFF;
            }

            /// <summary>
            /// Returns a nonnegative random number less than the specified maximum.
            /// </summary>
            /// <returns>
            /// A 32-bit signed integer greater than or equal to zero, and less than <paramref name="maxValue"/>; that is, the range of return values ordinarily includes zero but not <paramref name="maxValue"/>. However, if <paramref name="maxValue"/> equals zero, <paramref name="maxValue"/> is returned.
            /// </returns>
            /// <param name="maxValue">The exclusive upper bound of the random number to be generated. <paramref name="maxValue"/> must be greater than or equal to zero.</param>
            /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="maxValue"/> is less than zero.</exception>
            public override int Next(int maxValue)
            {
                if (maxValue < 0) throw new ArgumentOutOfRangeException("maxValue");
                return Next(0, maxValue);
            }

            /// <summary>
            /// Returns a random number within a specified range.
            /// </summary>
            /// <returns>
            /// A 32-bit signed integer greater than or equal to <paramref name="minValue"/> and less than <paramref name="maxValue"/>; that is, the range of return values includes <paramref name="minValue"/> but not <paramref name="maxValue"/>. If <paramref name="minValue"/> equals <paramref name="maxValue"/>, <paramref name="minValue"/> is returned.
            /// </returns>
            /// <param name="minValue">The inclusive lower bound of the random number returned.</param>
            /// <param name="maxValue">The exclusive upper bound of the random number returned. <paramref name="maxValue"/> must be greater than or equal to <paramref name="minValue"/>.</param>
            /// <exception cref="T:System.ArgumentOutOfRangeException"><paramref name="minValue"/> is greater than <paramref name="maxValue"/>.</exception>
            public override int Next(int minValue, int maxValue)
            {
                if (minValue > maxValue) throw new ArgumentOutOfRangeException("minValue");
                if (minValue == maxValue) return minValue;

                long diff = maxValue - minValue;
                long max = (1 + (long)uint.MaxValue);
                long remainder = max % diff;

                while (true)
                {
                    cryptoProvider.GetBytes(uint32Buffer);
                    uint rand = BitConverter.ToUInt32(uint32Buffer, 0);
                    if (rand < max - remainder)
                    {
                        return (int)(minValue + (rand % diff));
                    }
                }
            }

            /// <summary>
            /// Returns a random number between 0.0 and 1.0.
            /// </summary>
            /// <returns>
            /// A double-precision floating point number greater than or equal to 0.0, and less than 1.0.
            /// </returns>
            public override double NextDouble()
            {
                cryptoProvider.GetBytes(uint32Buffer);
                uint rand = BitConverter.ToUInt32(uint32Buffer, 0);
                return rand / (1.0 + uint.MaxValue);
            }

            /// <summary>
            /// Fills the elements of a specified array of bytes with random numbers.
            /// </summary>
            /// <param name="buffer">An array of bytes to contain random numbers.</param>
            /// <exception cref="T:System.ArgumentNullException"><paramref name="buffer"/> is null.
            public override void NextBytes(byte[] buffer)
            {
                if (buffer == null) throw new ArgumentNullException("buffer");
                cryptoProvider.GetBytes(buffer);
            }

            public void Dispose()
            {
                InternalDispose();
            }

            ~CryptoRandom()
            {
                InternalDispose();
            }

            void InternalDispose()
            {
                if (cryptoProvider != null)
                {
                    cryptoProvider.Dispose();
                    cryptoProvider = null;
                }
            }
        }
    }
}
