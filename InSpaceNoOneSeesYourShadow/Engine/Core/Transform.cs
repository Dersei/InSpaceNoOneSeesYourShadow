using System;
using System.Collections.Generic;
using OpenTK;

namespace InSpaceNoOneSeesYourShadow.Engine.Core
{
    public class Transform
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public Matrix4 WorldMatrix => CreateWorldMatrix();

        private Matrix4 CreateWorldMatrix()
        {
            var matrix = Matrix4.Identity;
            matrix *= Matrix4.CreateScale(Scale);
            matrix *= Matrix4.CreateRotationX(Rotation.X) *
                      Matrix4.CreateRotationY(Rotation.Y) *
                      Matrix4.CreateRotationZ(Rotation.Z);
            matrix *= Matrix4.CreateTranslation(Position);
            if (Parent != null)
            {
                matrix *= Parent.WorldMatrix;
            }
            return matrix;
        }

        public Transform Parent;

        public List<Transform> Children = new List<Transform>();
        public Func<float, Vector3> PositionModifier { get; set; }
        public Func<float, Vector3> RotationModifier { get; set; }
        public Func<float, Vector3> ScaleModifier { get; set; }

        public Transform(Vector3 position, Vector3 rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
            PositionModifier = _ => Position;
            RotationModifier = _ => Rotation;
            ScaleModifier = _ => Scale;
        }

        public void Update(float value)
        {
            Position = PositionModifier(value);
            Rotation = RotationModifier(value);
            Scale = ScaleModifier(value);
            //foreach (var child in Children)
            //{
            //    child.Update(value);
            //}
        }

        public void AddChild(Transform transform)
        {
            throw new NotSupportedException();
            //Children.Add(transform);
            //transform.Parent = this;
        }
    }
}
