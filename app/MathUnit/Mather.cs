using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace MathUnit
{
    public static class Mather
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Mod(double x, double y)
        {
            // C# does not have true modulo operator, only remainder, so we have to take care of negative numbers
            x %= y; 
            return x < 0 ? x + y : x;
        }
    }
}
