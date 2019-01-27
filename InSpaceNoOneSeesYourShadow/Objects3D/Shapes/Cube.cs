using System.Collections.Generic;
using OpenTK;

namespace InSpaceNoOneSeesYourShadow.Objects3D.Shapes
{
    internal class Cube : Volume
    {
        public Cube()
        {
            VerticesCount = 8;
            IndicesCount = 36;
            ColorDataCount = 8;
        }

        public override Vector3[] GetVertices()
        {
            var thisVertices = new List<Vector3> {new Vector3(-0.5f, -0.5f,  -0.5f),
                new Vector3(0.5f, -0.5f,  -0.5f),
                new Vector3(0.5f, 0.5f,  -0.5f),
                new Vector3(-0.5f, 0.5f,  -0.5f),
                new Vector3(-0.5f, -0.5f,  0.5f),
                new Vector3(0.5f, -0.5f,  0.5f),
                new Vector3(0.5f, 0.5f,  0.5f),
                new Vector3(-0.5f, 0.5f,  0.5f),
            };
            foreach (var child in Children)
            {
                thisVertices.AddRange(child.GetVertices());
            }

            return thisVertices.ToArray();
        }

        public override int[] GetIndices(int offset = 0)
        {
            var indices = new List<int>(){
                //left
                0, 2, 1,
                0, 3, 2,
                //back
                1, 2, 6,
                6, 5, 1,
                //right
                4, 5, 6,
                6, 7, 4,
                //top
                2, 3, 6,
                6, 3, 7,
                //front
                0, 7, 3,
                0, 4, 7,
                //bottom
                0, 1, 5,
                0, 5, 4
            };

            if (offset != 0)
            {
                for (int i = 0; i < indices.Count; i++)
                {
                    indices[i] += offset;
                }
            }

            var off = VerticesCount + offset;
            foreach (var child in Children)
            {
                indices.AddRange(child.GetIndices(off));
                off += child.VerticesCount;
            }

            return indices.ToArray();
        }

        public override Vector3[] GetColorData()
        {
            var colorData = new List<Vector3>() {
                new Vector3(1f, 0f, 0f),
                new Vector3( 0f, 0f, 1f),
                new Vector3( 0f, 1f, 0f),
                new Vector3( 1f, 0f, 0f),
                new Vector3( 0f, 0f, 1f),
                new Vector3( 0f, 1f, 0f),
                new Vector3( 1f, 0f, 0f),
                new Vector3( 0f, 0f, 1f)
            };

            foreach (var child in Children)
            {
                colorData.AddRange(child.GetColorData());
            }

            return colorData.ToArray();
        }

        public override void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
            if (Parent != null)
            {
                ModelMatrix *= Parent.ModelMatrix;
            }
            foreach (var volume in Children)
            {
                volume.CalculateModelMatrix();
            }
        }

        public override void UpdateMatrices(Matrix4 newValue)
        {
            ViewProjectionMatrix = newValue;
            ModelViewProjectionMatrix = ModelMatrix * ViewProjectionMatrix;
            foreach (var child in Children)
            {
                child.UpdateMatrices(newValue);
            }
        }

        public override Vector2[] GetTextureCoords()
        {
            return new Vector2[] { };
        }
    }
}
