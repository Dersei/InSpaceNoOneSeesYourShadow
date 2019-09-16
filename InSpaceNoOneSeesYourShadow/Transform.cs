using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace InSpaceNoOneSeesYourShadow
{
    public class Transform
    {
        public Vector3 Position { get; set; } = Vector3.Zero;
        public Vector3 Rotation { get; set; } = Vector3.Zero;
        public Vector3 Scale { get; set; } = Vector3.Zero;

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
            foreach (var child in Children)
            {
                child.Update(value);
            }
        }

        public void AddChild(Transform transform)
        {
            Children.Add(transform);
        }
    }
}
