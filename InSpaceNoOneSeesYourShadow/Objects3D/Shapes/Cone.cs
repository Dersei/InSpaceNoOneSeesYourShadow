using System;
using System.Collections.Generic;
using OpenTK;

namespace InSpaceNoOneSeesYourShadow.Objects3D.Shapes
{
    sealed class Cone : Volume
    {
        public Cone()
        {
            VerticesCount = 24;
            IndicesCount = 24;
            ColorDataCount = 24;
        }

        public int Multiplier { get; set; } = 0;

        public override Vector3[] GetVertices()
        {
            var pt = new List<Vector3>();
            var nt = new List<Vector3>();
            float r1 = 0.5f;
            float r2 = 0.0f;
            float h = 3;
            float nPhi = 50 * Multiplier;
            float Phi = 0;
            float dPhi = (float)(2 * Math.PI / (nPhi - 1));
            float Nx = r1 - r2;
            float Ny = h;
            float N = (float) Math.Sqrt(Nx * Nx + Ny * Ny);
            Nx /= N; Ny /= N;
            for (var i = 0; i < nPhi; i++)
            {
                float cosPhi = (float)Math.Cos(Phi);
                float sinPhi = (float)Math.Sin(Phi);
                float cosPhi2 = (float)Math.Cos(Phi + dPhi / 2);
                float sinPhi2 = (float)Math.Sin(Phi + dPhi / 2);
                pt.Add(new Vector3(-h / 2, cosPhi * r1, sinPhi * r1));   // points
                nt.Add(new Vector3(Nx, Ny * cosPhi, Ny * sinPhi));         // normals
                pt.Add(new Vector3(h / 2, cosPhi2 * r2, sinPhi2 * r2));  // points
                nt.Add(new Vector3(Nx, Ny * cosPhi2, Ny * sinPhi2));       // normals
                Phi += dPhi;
            }

            var vertices = pt;
            _normals = nt.ToArray();
            IndicesCount = vertices.Count;
            VerticesCount = vertices.Count;
            ColorDataCount = vertices.Count;

            foreach (var child in Children)
            {
                vertices.AddRange(child.GetVertices());
            }

            return vertices.ToArray();
        }

        public override int[] GetIndices(int offset = 0)
        {
            List<int> indices = new List<int>();
            for (int i = 0; i < IndicesCount; i++)
            {
                indices.Add(i);
            }

            if (offset != 0)
            {
                for (int i = 0; i < indices.Count; i++)
                {
                    indices[i] += offset;
                }
            }

            foreach (var child in Children)
            {
                indices.AddRange(child.GetIndices(VerticesCount));
            }

            return indices.ToArray();
        }

        public override Vector3[] GetColorData()
        {
            List<Vector3> colors = new List<Vector3>();
            for (int i = 0; i < ColorDataCount; i++)
            {
                colors.Add(new Vector3(i / 64f, 1f, i / 64f));
            }

            foreach (var child in Children)
            {
                colors.AddRange(child.GetColorData());
            }

            return colors.ToArray();
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
