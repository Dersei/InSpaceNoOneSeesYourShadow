using System;
using OpenTK.Mathematics;

namespace InSpaceNoOneSeesYourShadow.Engine.Cameras
{
    public class GameCamera : Camera
    {
        public GameCamera()
        {
            Position = Vector3.Zero;
            Orientation = new Vector3((float)Math.PI, 0f, 0f);
            MoveSpeed = 0.2f;
            MouseSensitivity = 0.01f;
        }

        /// <summary>
        /// Create a view matrix for this Camera
        /// </summary>
        /// <returns>A view matrix to look in the camera's direction</returns>
        protected override Matrix4 GetViewMatrix()
        {
            //Vector3 lookAt = new Vector3
            //{
            //    X = (float)(Math.Sin(Orientation.X) * Math.Cos(Orientation.Y)),
            //    Y = (float)Math.Sin(Orientation.Y),
            //    Z = (float)(Math.Cos(Orientation.X) * Math.Cos(Orientation.Y))
            //};


            return Matrix4.LookAt(Position, Vector3.Zero, Vector3.UnitY);
        }

        /// <summary>
        /// Offset the position of the camera in coordinates relative to its current orientation
        /// </summary>
        /// <param name="x">Movement along the camera ground (left/right)</param>
        /// <param name="y">Movement along the camera axis (forward)</param>
        /// <param name="z">Height to move</param>
        public override void Move(float x, float y, float z)
        {
            Vector3 offset = new Vector3();
            Vector3 forward = new Vector3((float)Math.Sin(Orientation.X), 0, (float)Math.Cos(Orientation.X));
            Vector3 right = new Vector3(-forward.Z, 0, forward.X);

            offset += x * right;
            offset += y * forward;
            offset.Y += z;

            offset.NormalizeFast();
            offset = Vector3.Multiply(offset, MoveSpeed);

            Position += offset;
        }

        /// <summary>
        /// Adds rotation from mouse movement to camera orientation
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public override void AddRotation(float x, float y)
        {
            x *= MouseSensitivity;
            y *= MouseSensitivity;

            Orientation.X = (Orientation.X + x) % ((float)Math.PI * 2.0f);
            Orientation.Y = Math.Max(Math.Min(Orientation.Y + y, (float)Math.PI / 2.0f - 0.1f), (float)-Math.PI / 2.0f + 0.1f);
        }
    }
}
