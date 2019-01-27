﻿using System;
using OpenTK;

namespace InSpaceNoOneSeesYourShadow.Helpers.Cameras
{
    internal class BasicCamera
    {
        public Vector3 Position = new Vector3(0, 0, 0);
        public Vector3 Orientation = new Vector3((float)Math.PI, 0f, 0f);
        public float MoveSpeed = 0.2f;
        public float MouseSensitivity = 0.01f;

        public Matrix4 GetViewMatrix()
        {
            var lookAt = new Vector3
            {
                X = (float)(Math.Sin(Orientation.X) * Math.Cos(Orientation.Y)),
                Y = (float)Math.Sin(Orientation.Y),
                Z = (float)(Math.Cos(Orientation.X) * Math.Cos(Orientation.Y))
            };


            return Matrix4.LookAt(Position - lookAt, Position + lookAt, Vector3.UnitY);
        }

        public void Move(float x, float y, float z)
        {
            var offset = new Vector3();

            var forward = new Vector3((float)Math.Sin((float)Orientation.X), 0, (float)Math.Cos((float)Orientation.X));
            var right = new Vector3(-forward.Z, 0, forward.X);

            offset += x * right;
            offset += y * forward;
            offset.Y += z;

            offset.NormalizeFast();
            offset = Vector3.Multiply(offset, MoveSpeed);

            Position += offset;
        }

        public void AddRotation(float x, float y)
        {
            x = x * MouseSensitivity;
            y = y * MouseSensitivity;

            Orientation.X = (Orientation.X + x) % ((float)Math.PI * 2.0f);
            Orientation.Y = Math.Max(Math.Min(Orientation.Y + y, (float)Math.PI / 2.0f - 0.1f), (float)-Math.PI / 2.0f + 0.1f);
        }
    }
}