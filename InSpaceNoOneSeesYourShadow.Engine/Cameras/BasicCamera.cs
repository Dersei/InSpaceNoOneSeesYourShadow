using System;
using OpenTK.Mathematics;

namespace InSpaceNoOneSeesYourShadow.Engine.Cameras
{
    internal class BasicCamera : Camera
    {
        public BasicCamera()
        {
            Position = new Vector3(0, 0, 0);
            Orientation = new Vector3((float)Math.PI, 0f, 0f);
            MoveSpeed = 0.2f;
            MouseSensitivity = 0.01f;
        }

        protected override Matrix4 GetViewMatrix()
        {
            var lookAt = new Vector3
            {
                X = (float)(Math.Sin(Orientation.X) * Math.Cos(Orientation.Y)),
                Y = (float)Math.Sin(Orientation.Y),
                Z = (float)(Math.Cos(Orientation.X) * Math.Cos(Orientation.Y))
            };


            return Matrix4.LookAt(Position - lookAt, Position + lookAt, Vector3.UnitY);
        }
        
        public override void Move(float x, float y, float z)
        {
            var offset = new Vector3();

            var forward = new Vector3((float)Math.Sin(Orientation.X), 0, (float)Math.Cos(Orientation.X));
            var right = new Vector3(-forward.Z, 0, forward.X);

            offset += x * right;
            offset += y * forward;
            offset.Y += z;

            offset.NormalizeFast();
            offset = Vector3.Multiply(offset, MoveSpeed);

            Position += offset;
        }

        public override void AddRotation(float x, float y)
        {
            x *= MouseSensitivity;
            y *= MouseSensitivity;

            Orientation.X = (Orientation.X + x) % ((float)Math.PI * 2.0f);
            Orientation.Y = Math.Max(Math.Min(Orientation.Y + y, (float)Math.PI / 2.0f - 0.1f), (float)-Math.PI / 2.0f + 0.1f);
        }
    }
}
