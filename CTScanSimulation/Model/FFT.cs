using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CTScanSimulation.Model
{
    /// <summary>
    /// Provides forward and reverse FFT using Cooley–Tukey algorithm.
    /// https://en.wikipedia.org/wiki/Cooley–Tukey_FFT_algorithm
    /// </summary>
    internal static class FFT
    {
        /// <summary>
        /// Represents direction of transformation.
        /// </summary>
        private enum Direction
        {
            Forward, Inverse
        }

        /// <summary>
        /// Performs forward FFT.
        /// </summary>
        /// <param name="input">Input array of complex numbers.</param>
        /// <returns>Result of forward FFT.</returns>
        public static Complex[] Forward(Complex[] input)
        {
            return IterativeFFT(input, Direction.Forward);
        }

        /// <summary>
        /// Performs inverse FFT.
        /// </summary>
        /// <param name="input">Input array of complex numbers.</param>
        /// <returns>Result of inverse FFT.</returns>
        public static Complex[] Inverse(Complex[] input)
        {
            return IterativeFFT(input, Direction.Inverse);
        }

        /// <summary>
        /// Reverses the order of bits in <c>int</c>.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="significantBits">How many bits to swap in <c>uint</c>.</param>
        /// <returns></returns>
        private static long BitReverse(uint value, int significantBits)
        {
            long result = 0;
            for (int i = 0; i < significantBits / 2; i++)
            {
                int bitToSwap = significantBits - i - 1;
                result |= ((value & (1 << i)) >> i) << bitToSwap;
                result |= ((value & (1 << bitToSwap)) >> bitToSwap) << i;
            }
            return result;
        }

        /// <summary>
        /// Performs bit-reversal permutation of given array.
        /// https://en.wikipedia.org/wiki/Bit-reversal_permutation
        /// </summary>
        /// <param name="input"></param>
        /// <returns>New array which is bit-reversal permutation of input.</returns>
        private static Complex[] BitReverseCopy(Complex[] input)
        {
            int length = input.Length;
            var result = new Complex[length];
            int significantBits = (int)(Math.Log(length) / Math.Log(2));
            for (uint i = 0; i < length; i++)
            {
                result[BitReverse(i, significantBits)] = input[i];
            }
            return result;
        }

        /// <summary>
        /// Checks whether given number is a power of 2.
        /// </summary>
        /// <param name="x"></param>
        /// <returns>True if number is a power of 2, false otherwise.</returns>
        private static bool IsPowerOfTwo(long x)
        {
            return (x & (x - 1)) == 0;
        }

        /// <summary>
        /// Performs iterative FFT on given array.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="direction">Forward or inverse.</param>
        /// <returns>FFT of given array.</returns>
        private static Complex[] IterativeFFT(Complex[] input, Direction direction)
        {
            int length = input.Length;

            if (!IsPowerOfTwo(length))
            {
                int power = (int)Math.Ceiling(Math.Log(length) / Math.Log(2));
                Array.Resize(ref input, (int)Math.Pow(2, power));
                length = input.Length;
            }

            var result = BitReverseCopy(input);
            int m = 1;
            int log = (int)(Math.Log(length) / Math.Log(2));
            for (int i = 0; i < log; i++)
            {
                m *= 2;
                Complex index = (direction == Direction.Inverse) ? new Complex(0, -2 * Math.PI / m) : new Complex(0, 2 * Math.PI / m);
                Complex omegaM = Complex.Exp(index);
                for (int k = 0; k < length; k += m)
                {
                    Complex omega = 1;
                    for (int j = 0; j < m / 2; j++)
                    {
                        var t = omega * result[k + j + m / 2];
                        var u = result[k + j];
                        result[k + j] = u + t;
                        result[k + j + m / 2] = u - t;
                        omega *= omegaM;
                    }
                }
            }
            if (direction == Direction.Inverse) result.Select(x => x / length);
            return result;
        }
    }
}