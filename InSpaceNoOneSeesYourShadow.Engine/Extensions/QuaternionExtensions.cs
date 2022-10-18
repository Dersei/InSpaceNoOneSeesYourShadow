using System;
using OpenTK.Mathematics;

namespace InSpaceNoOneSeesYourShadow.Engine.Extensions
{
    public static class QuaternionExtensions
    {
        public static float ArcTanAngle(float x, float y)
        {
            if (x.IsAbout(0))
            {
                if (y.IsAbout(1))
                    return MathHelper.PiOver2;
                return -MathHelper.PiOver2;
            }

            if (x > 0)
                return (float)Math.Atan(y / x);
            if (x < 0)
            {
                if (y > 0)
                    return (float)Math.Atan(y / x) + MathHelper.Pi;
                return (float)Math.Atan(y / x) - MathHelper.Pi;
            }

            return 0;
        }

        //returns Euler angles that point from one point to another
        public static Vector3 AngleTo(Vector3 from, Vector3 location)
        {
            var angle = new Vector3();
            var v3 = Vector3.Normalize(location - from);
            angle.X = (float)Math.Asin(v3.Y);
            angle.Y = ArcTanAngle(-v3.Z, -v3.X);
            return angle;
        }

        public static void ToEuler(float x, float y, float z, float w, out Vector3 result)
        {
            var rotation = new Quaternion(x, y, z, w);
            var forward = Vector3.Transform(-Vector3.UnitZ, rotation);
            var up = Vector3.Transform(Vector3.UnitY, rotation);
            result = AngleTo(new Vector3(), forward);
            if (result.X.IsAbout(MathHelper.PiOver2))
            {
                result.Y = ArcTanAngle(up.Z, up.X);
                result.Z = 0;
            }
            else if (result.X.IsAbout(-MathHelper.PiOver2))
            {
                result.Y = ArcTanAngle(-up.Z, -up.X);
                result.Z = 0;
            }
            else
            {
                up = Vector3.Transform(up, Matrix3.CreateRotationY(-result.Y).ExtractRotation());
                up = Vector3.Transform(up, Matrix3.CreateRotationX(-result.X).ExtractRotation());
                result.Z = ArcTanAngle(up.Y, -up.X);
            }
        }

        public static void ToEuler(this Quaternion quaternion, out Vector3 result)
        {
            ToEuler(quaternion.X, quaternion.Y, quaternion.Z, quaternion.W, out result);
        }

        public static Vector3 ToEuler(this Quaternion quaternion)
        {
            ToEuler(quaternion, out var result);
            return result;
        }

        public static Quaternion Euler(float x, float y, float z) => CreateFromYawPitchRoll(y, x, z);

        public static Quaternion Euler(Vector3 rotation) => CreateFromYawPitchRoll(rotation.Y, rotation.X, rotation.Z);

        public static Quaternion ToQuaternionFromEuler(this Vector3 @this) =>
            CreateFromYawPitchRoll(@this.Y, @this.X, @this.Z);

        /// <summary>
        /// Creates a new <see cref="Quaternion"/> from the specified yaw, pitch and roll angles.
        /// </summary>
        /// <param name="yaw">Yaw around the y axis in radians.</param>
        /// <param name="pitch">Pitch around the x axis in radians.</param>
        /// <param name="roll">Roll around the z axis in radians.</param>
        /// <returns>A new quaternion from the concatenated yaw, pitch, and roll angles.</returns>
        private static Quaternion CreateFromYawPitchRoll(float yaw, float pitch, float roll)
        {
            var halfRoll = roll * 0.5f;
            var halfPitch = pitch * 0.5f;
            var halfYaw = yaw * 0.5f;

            var sinRoll = (float)Math.Sin(halfRoll);
            var cosRoll = (float)Math.Cos(halfRoll);
            var sinPitch = (float)Math.Sin(halfPitch);
            var cosPitch = (float)Math.Cos(halfPitch);
            var sinYaw = (float)Math.Sin(halfYaw);
            var cosYaw = (float)Math.Cos(halfYaw);

            return new Quaternion((cosYaw * sinPitch * cosRoll) + (sinYaw * cosPitch * sinRoll),
                (sinYaw * cosPitch * cosRoll) - (cosYaw * sinPitch * sinRoll),
                (cosYaw * cosPitch * sinRoll) - (sinYaw * sinPitch * cosRoll),
                (cosYaw * cosPitch * cosRoll) + (sinYaw * sinPitch * sinRoll));
        }
    }
}

