using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using InSpaceNoOneSeesYourShadow.Engine.Core;
using InSpaceNoOneSeesYourShadow.Engine.Helpers;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace InSpaceNoOneSeesYourShadow.Engine.Objects3D.Shapes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoords;
    }

    public class ObjVolume : Volume
    {
        public PBRValues PbrValues { get; set; }

        public struct PBRValues
        {
            public float AO { get; set; }
            public float Metallic { get; set; }
            public float Roughness { get; set; }
            public float ReflectionStrength { get; set; }
            public float Refraction { get; set; }
        }

        private readonly List<(FaceVertex, FaceVertex, FaceVertex)> _faces = new List<(FaceVertex, FaceVertex, FaceVertex)>();
        public List<(FaceVertex, FaceVertex, FaceVertex)> Faces => _faces;
        public override int VerticesCount => _faces.Count * 3;

        public override int IndicesCount => _faces.Count * 3;

        public override int ColorDataCount => _faces.Count * 3;

        public override int TextureCoordsCount => _faces.Count * 3;

        public List<Vertex> VerticesStruct = new List<Vertex>();

        public int VAO;
        public int VBO;
        public int EBO;

        public ShaderProgram VolumeShader { get; set; }

        public unsafe void Bind(bool onlyStructs = false)
        {
            var normals = GetNormals();
            var texcoords = GetTextureCoords();
            var vertices = GetVertices();

            for (var i = 0; i < vertices.Length; i++)
            {
                VerticesStruct.Add(new Vertex()
                {
                    Normal = normals[i],
                    TexCoords = texcoords[i],
                    Position = vertices[i]
                });
            }

            if (onlyStructs) return;


            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            EBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            GL.BufferData(BufferTarget.ArrayBuffer, VerticesStruct.Count * sizeof(Vertex), VerticesStruct.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, GetIndices().Length * sizeof(int), GetIndices(), BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(VolumeShader.GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, sizeof(Vertex), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(VolumeShader.GetAttribute("vNormal"), 3, VertexAttribPointerType.Float, true, sizeof(Vertex), Vector3.SizeInBytes);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(VolumeShader.GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, sizeof(Vertex), Vector3.SizeInBytes * 2);

            GL.BindVertexArray(0);

        }

        /// <summary>
        /// Get vertices for this object
        /// </summary>
        /// <returns></returns>
        public override Vector3[] GetVertices()
        {
            var vertices = new List<Vector3>();

            foreach (var face in _faces)
            {
                vertices.Add(face.Item1.Position);
                vertices.Add(face.Item2.Position);
                vertices.Add(face.Item3.Position);
            }

            //foreach (var child in Children)
            //{
            //    vertices.AddRange(child.GetVertices());
            //}

            return vertices.ToArray();
        }

        /// <summary>
        /// Get indices to draw this object
        /// </summary>
        /// <param name="offset">value to number first vertex in object</param>
        /// <returns>Array of indices offset to match buffered data</returns>
        public override int[] GetIndices(int offset = 0)
        {
            var indices = Enumerable.Range(offset, IndicesCount).ToList();
            var off = VerticesCount + offset;

            //foreach (var child in Children)
            //{
            //    indices.AddRange(child.GetIndices(off));
            //    off += child.VerticesCount;
            //}

            return indices.ToArray();
        }

        /// <summary>
        /// Get color data.
        /// </summary>
        /// <returns></returns>
        public override Vector3[] GetColorData()
        {
            var colors = new Vector3[ColorDataCount].ToList();
            //foreach (var child in Children)
            //{
            //    colors.AddRange(child.GetColorData());
            //}
            return colors.ToArray();
        }

        /// <summary>
        /// Get texture coordinates
        /// </summary>
        /// <returns></returns>
        public override Vector2[] GetTextureCoords()
        {
            var coords = new List<Vector2>();

            foreach (var face in _faces)
            {
                coords.Add(face.Item1.TextureCoords);
                coords.Add(face.Item2.TextureCoords);
                coords.Add(face.Item3.TextureCoords);
            }

            //foreach (var child in Children)
            //{
            //    coords.AddRange(child.GetTextureCoords());
            //}
            return coords.ToArray();
        }


        /// <summary>
        /// Calculates the model matrix from transforms
        /// </summary>
        public override void CalculateModelMatrix(Transform transform)
        {
            ModelMatrix = Matrix4.CreateScale(transform.Scale) * Matrix4.CreateRotationX(transform.Rotation.X) * Matrix4.CreateRotationY(transform.Rotation.Y) * Matrix4.CreateRotationZ(transform.Rotation.Z) * Matrix4.CreateTranslation(transform.Position);
            //if (Parent != null)
            //{
            //    ModelMatrix *= Parent.ModelMatrix;
            //}
            //foreach (var volume in Children)
            //{
            //    volume.CalculateModelMatrix();
            //}
        }

        public override void UpdateMatrices(Matrix4 newValue)
        {
            ViewProjectionMatrix = newValue;
            ModelViewProjectionMatrix = ModelMatrix * ViewProjectionMatrix;

            //foreach (var child in Children)
            //{
            //    child.UpdateMatrices(newValue);
            //}
        }

        
        public override Vector3[] GetNormals()
        {
            if (base.GetNormals().Length > 0)
            {
                return base.GetNormals();
            }

            var normals = new List<Vector3>();

            foreach (var face in _faces)
            {
                normals.Add(face.Item1.Normal);
                normals.Add(face.Item2.Normal);
                normals.Add(face.Item3.Normal);
            }

            //foreach (var child in Children)
            //{
            //    normals.AddRange(child.GetNormals());
            //}

            return normals.ToArray();
        }

        public override int NormalCount => _faces.Count * 3;
    }

    public struct FaceVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoords;

        public FaceVertex(Vector3 pos, Vector3 norm, Vector2 texCoords)
        {
            Position = pos;
            Normal = norm;
            TextureCoords = texCoords;
        }
    }
}
