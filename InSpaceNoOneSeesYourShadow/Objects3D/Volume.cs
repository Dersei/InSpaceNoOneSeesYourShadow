using System;
using System.Collections.Generic;
using InSpaceNoOneSeesYourShadow.Helpers;
using OpenTK;

namespace InSpaceNoOneSeesYourShadow.Objects3D
{
    public abstract class Volume
    {
        //public Vector3 Position { get; set; } = Vector3.Zero;
        //public Vector3 Rotation { get; set; } = Vector3.Zero;
        //public Vector3 Scale { get; set; } = Vector3.Zero;
        //public Volume Parent;
        //public List<Volume> Children = new List<Volume>();
        //public Func<float, Vector3> PositionModifier { get; set; }
        //public Func<float, Vector3> RotationModifier { get; set; }
        //public Func<float, Vector3> ScaleModifier { get; set; }

        public GameObject GameObject;

        public Volume()
        {
            //PositionModifier = _ => Position;
            //RotationModifier = _ => Rotation;
            //ScaleModifier = _ => Scale;
        }

        public ShaderProgram ShaderId { get; set; }
        public void Update(float value)
        {
            //Position = PositionModifier(value);
            //Rotation = RotationModifier(value);
            //Scale = ScaleModifier(value);
            //foreach (var child in Children)
            //{
            //    child.Update(value);
            //}
        }

        public virtual int VerticesCount { get; set; }
        public virtual int IndicesCount { get; set; }
        public virtual int ColorDataCount { get; set; }

        public Matrix4 ModelMatrix = Matrix4.Zero;
        public Matrix4 ViewProjectionMatrix = Matrix4.Zero;
        public Matrix4 ModelViewProjectionMatrix = Matrix4.Zero;


        public Material Material = new Material();


        public abstract Vector3[] GetVertices();
        public abstract int[] GetIndices(int offset = 0);
        public abstract Vector3[] GetColorData();
        public abstract void CalculateModelMatrix();
        public abstract void UpdateMatrices(Matrix4 newValue);
        public Vector3[] _normals = new Vector3[0];
        public virtual int NormalCount => _normals.Length;

        public void AddChild(Volume child)
        {
            //child.Parent = this;
            //Children.Add(child);
        }

        public virtual Vector3[] GetNormals()
        {
            return _normals;
        }

        public void CalculateNormals()
        {
            Vector3[] normals = new Vector3[VerticesCount];
            Vector3[] verts = GetVertices();
            int[] inds = GetIndices();

            // Compute normals for each face
            for (int i = 0; i < IndicesCount; i += 3)
            {
                Vector3 v1 = verts[inds[i]];
                Vector3 v2 = verts[inds[i + 1]];
                Vector3 v3 = verts[inds[i + 2]];

                // The normal is the cross product of two sides of the triangle
                normals[inds[i]] += Vector3.Cross(v2 - v1, v3 - v1);
                normals[inds[i + 1]] += Vector3.Cross(v2 - v1, v3 - v1);
                normals[inds[i + 2]] += Vector3.Cross(v2 - v1, v3 - v1);
            }

            for (int i = 0; i < NormalCount; i++)
            {
                normals[i] = normals[i].Normalized();
            }

            _normals = normals;
        }

        public bool IsTextured = false;
        public int TextureId;
        public virtual int TextureCoordsCount { get; set; }
        public abstract Vector2[] GetTextureCoords();
    }
}
