using System;

namespace InSpaceNoOneSeesYourShadow.Engine.Extensions
{
    public static class VariousExtensions
    {
        public static bool IsAbout(this float @this, float value, float precisionMultiplier = 8)
        {
            const double epsilon = 1.175494E-38;
            return Math.Abs(value - @this) < Math.Max(1E-06 * Math.Max(Math.Abs(@this), Math.Abs(value)), epsilon * precisionMultiplier);
        }

        public static bool IsAbout(this double @this, double value, double precisionMultiplier = 8)
        {
            const double epsilon = 1.175494E-38;
            return Math.Abs(value - @this) < Math.Max(1E-06 * Math.Max(Math.Abs(@this), Math.Abs(value)), epsilon * precisionMultiplier);
        }

        public static bool Contains(this string? source, string toCheck, StringComparison comp)
        {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static string? SafeSubstring(this string? @this, int endIndex)
        {
            return @this?[..Math.Min(endIndex, @this.Length)];
        }
    }

}
