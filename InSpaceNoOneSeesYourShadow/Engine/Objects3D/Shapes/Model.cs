using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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

    public class Model
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

        public List<(FaceVertex, FaceVertex, FaceVertex)> Faces { get; } =
            new List<(FaceVertex, FaceVertex, FaceVertex)>();

        public int VerticesCount => Faces.Count * 3;

        public int IndicesCount => Faces.Count * 3;

        public int ColorDataCount => Faces.Count * 3;

        public int TextureCoordsCount => Faces.Count * 3;

        public List<Vertex> VerticesStruct = new List<Vertex>();
        public int NormalCount => Faces.Count * 3;
        public int TextureId { get; set; }
        public Material Material { get; set; }

        public int VAO;
        public int VBO;
        public int EBO;

        public ShaderProgram VolumeShader { get; set; }

        public unsafe void Bind(bool onlyStructs = false)
        {
            var normals = GetNormals();
            var textureCoords = GetTextureCoords();
            var vertices = GetVertices();

            for (var i = 0; i < vertices.Length; i++)
            {
                VerticesStruct.Add(new Vertex()
                {
                    Normal = normals[i],
                    TexCoords = textureCoords[i],
                    Position = vertices[i]
                });
            }

            if (onlyStructs) return;


            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            EBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            GL.BufferData(BufferTarget.ArrayBuffer, VerticesStruct.Count * sizeof(Vertex), VerticesStruct.ToArray(),
                BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, GetIndices().Length * sizeof(int), GetIndices(),
                BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(VolumeShader.GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false,
                sizeof(Vertex), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(VolumeShader.GetAttribute("vNormal"), 3, VertexAttribPointerType.Float, true,
                sizeof(Vertex), Vector3.SizeInBytes);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(VolumeShader.GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true,
                sizeof(Vertex), Vector3.SizeInBytes * 2);

            GL.BindVertexArray(0);
        }

        /// <summary>
        /// Get vertices for this object
        /// </summary>
        /// <returns></returns>
        public Vector3[] GetVertices()
        {
            var vertices = new List<Vector3>();

            foreach (var face in Faces)
            {
                vertices.Add(face.Item1.Position);
                vertices.Add(face.Item2.Position);
                vertices.Add(face.Item3.Position);
            }

            return vertices.ToArray();
        }

        /// <summary>
        /// Get indices to draw this object
        /// </summary>
        /// <param name="offset">value to number first vertex in object</param>
        /// <returns>Array of indices offset to match buffered data</returns>
        public int[] GetIndices(int offset = 0) => Enumerable.Range(offset, IndicesCount).ToArray();

        /// <summary>
        /// Get color data.
        /// </summary>
        /// <returns></returns>
        public Vector3[] GetColorData() => new Vector3[ColorDataCount].ToArray();

        /// <summary>
        /// Get texture coordinates
        /// </summary>
        /// <returns></returns>
        public Vector2[] GetTextureCoords()
        {
            var coords = new List<Vector2>();

            foreach (var face in Faces)
            {
                coords.Add(face.Item1.TextureCoords);
                coords.Add(face.Item2.TextureCoords);
                coords.Add(face.Item3.TextureCoords);
            }

            return coords.ToArray();
        }


        public Vector3[] GetNormals()
        {
            var normals = new List<Vector3>();

            foreach (var face in Faces)
            {
                normals.Add(face.Item1.Normal);
                normals.Add(face.Item2.Normal);
                normals.Add(face.Item3.Normal);
            }

            return normals.ToArray();
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

   
}