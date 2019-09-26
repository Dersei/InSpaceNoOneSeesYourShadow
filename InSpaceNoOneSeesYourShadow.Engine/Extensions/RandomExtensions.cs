using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;

namespace InSpaceNoOneSeesYourShadow.Engine.Extensions
{
    public static class RandomExtensions
    {
        public static T RandomItem<T>(this List<T> @this)
        {
            if (@this.Count == 0)
                return default;
            var index = Basic.Random.Next(0, @this.Count);
            return @this[index];
        }

        public static T RandomItem<T>(this List<T> @this, Random random)
        {
            if (@this.Count == 0)
                return default;
            var index = random.Next(0, @this.Count);
            return @this[index];
        }


        public static T RandomItem<T>(this T[] @this)
        {
            if (@this.Length == 0)
                return default;
            var index = Basic.Random.Next(0, @this.Length);
            return @this[index];
        }

        public static T RandomItem<T>(this T[] @this, Random random)
        {
            if (@this.Length == 0)
                return default;
            var index = random.Next(0, @this.Length);
            return @this[index];
        }

        public static T RandomItem<T>(this IEnumerable<T> @this)
        {
            if (@this is List<T> list)
            {
                return list.RandomItem();
            }

            if (@this is T[] array)
            {
                return array.RandomItem();
            }

            var iterated = @this.ToList();
            if (iterated.Count == 0)
                return default;

            var index = Basic.Random.Next(0, iterated.Count());
            return iterated[index];
        }

        public static T2 RandomItem<T1, T2>(this Dictionary<T1, T2> @this)
        {
            if (@this.Count == 0)
                return default;
            var index = Basic.Random.Next(0, @this.Values.Count);
            return @this.Values.ElementAt(index);
        }

        public static float NextFloat(this Random @this, float minValue, float maxValue)
        {
            var value = (float)@this.NextDouble();
            return value * (maxValue - minValue) + minValue;
        }

        public static Vector3 NextVector3(this Random @this, Vector3 minValues, Vector3 maxValues)
        {
            var x = @this.NextFloat(minValues.X, maxValues.X);
            var y = @this.NextFloat(minValues.Y, maxValues.Y);
            var z = @this.NextFloat(minValues.Z, maxValues.Z);
            return new Vector3(x, y, z);
        }

        public static Vector3 NextColor(this Random @this)
        {
            var r = (float)@this.NextDouble();
            var g = (float)@this.NextDouble();
            var b = (float)@this.NextDouble();
            return new Vector3(r, g, b);
        }

        public static Vector3 NextColor255(this Random @this)
        {
            var r = @this.Next(0, 255);
            var g = @this.Next(0, 255);
            var b = @this.Next(0, 255);
            return new Vector3(r, g, b);
        }
    }
}
