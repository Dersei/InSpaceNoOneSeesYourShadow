using System.Collections.Generic;
using System.Linq;
using InSpaceNoOneSeesYourShadow.Engine.Helpers;
using InSpaceNoOneSeesYourShadow.Engine.Shaders;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace InSpaceNoOneSeesYourShadow.Engine.Objects3D
{
    public partial class Model
    {
        public PbrValues Pbr { get; set; }

        internal List<(FaceVertex, FaceVertex, FaceVertex)> Faces { get; } = new();

        private int VerticesCount => Faces.Count * 3;

        private int IndicesCount => Faces.Count * 3;

        private int ColorDataCount => Faces.Count * 3;

        private int TextureCoordsCount => Faces.Count * 3;

        private readonly List<Vertex> _verticesStruct = new();
        private int NormalCount => Faces.Count * 3;
        public Material? Material { get; set; }
        public Texture2D? Texture { get; set; }

        private int _vao;
        private int _vbo;
        private int _ebo;
        private ShaderProgram _volumeShader;

        public ShaderProgram VolumeShader
        {
            get => _volumeShader;
            set
            {
                _volumeShader = value;
                _volumeShader.Model = this;
            }
        }

        public unsafe void Bind(bool onlyStructs = false)
        {
            var normals = GetNormals();
            var textureCoords = GetTextureCoords();
            var vertices = GetVertices();

            for (var i = 0; i < vertices.Length; i++)
            {
                _verticesStruct.Add(new Vertex(vertices[i], normals[i], textureCoords[i]));
            }

            if (onlyStructs) return;

            _vao = GL.GenVertexArray();
            _vbo = GL.GenBuffer();
            _ebo = GL.GenBuffer();

            GL.BindVertexArray(_vao);

            GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo);

            GL.BufferData(BufferTarget.ArrayBuffer, _verticesStruct.Count * sizeof(Vertex), _verticesStruct.ToArray(),
                BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _ebo);
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
        private Vector3[] GetVertices()
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
        private int[] GetIndices(int offset = 0) => Enumerable.Range(offset, IndicesCount).ToArray();

        /// <summary>
        /// Get color data.
        /// </summary>
        /// <returns></returns>
        private Vector3[] GetColorData() => new Vector3[ColorDataCount].ToArray();

        /// <summary>
        /// Get texture coordinates
        /// </summary>
        /// <returns></returns>
        private Vector2[] GetTextureCoords()
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


        private Vector3[] GetNormals()
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

        public void BeginDraw()
        {
            VolumeShader.EnableVertexAttributesArrays();
            GL.UseProgram(VolumeShader.ProgramId);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Texture.ID);

            VolumeShader.Draw();
        }

        public void EndDraw()
        {
            GL.BindVertexArray(_vao);
            GL.DrawElements(PrimitiveType.Triangles, IndicesCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);
            VolumeShader.DisableVertexAttributesArrays();
        }
    }
}