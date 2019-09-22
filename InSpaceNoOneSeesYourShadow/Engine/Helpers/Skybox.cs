using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace InSpaceNoOneSeesYourShadow.Engine.Helpers
{
    public class Skybox
    {
        public Skybox(float size = 10.0f)
        {
            SkyboxVertices = new[]{
                // positions          
                -size,  size, -size,
                -size, -size, -size,
                size, -size, -size,
                size, -size, -size,
                size,  size, -size,
                -size,  size, -size,

                -size, -size,  size,
                -size, -size, -size,
                -size,  size, -size,
                -size,  size, -size,
                -size,  size,  size,
                -size, -size,  size,

                size, -size, -size,
                size, -size,  size,
                size,  size,  size,
                size,  size,  size,
                size,  size, -size,
                size, -size, -size,

                -size, -size,  size,
                -size,  size,  size,
                size,  size,  size,
                size,  size,  size,
                size, -size,  size,
                -size, -size,  size,

                -size,  size, -size,
                size,  size, -size,
                size,  size,  size,
                size,  size,  size,
                -size,  size,  size,
                -size,  size, -size,

                -size, -size, -size,
                -size, -size,  size,
                size, -size, -size,
                size, -size, -size,
                -size, -size,  size,
                size, -size,  size
            };

            SkyboxShaders = new ShaderProgram("_Resources/Shaders/Skybox/skybox.vert", "_Resources/Shaders/Skybox/skybox.frag", true);
            CreateSkybox();
        }

        public float[] SkyboxVertices { get; }

        public ShaderProgram SkyboxShaders;

        public Matrix4 View = Matrix4.Identity;
        public float AspectRatio = 1;

        private int _skyboxVao;
        private int _skyboxVbo;
        private int _cubeMapTexture;

        private void CreateSkybox()
        {
            _skyboxVao = GL.GenVertexArray();
            _skyboxVbo = GL.GenBuffer();
            GL.BindVertexArray(_skyboxVao);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _skyboxVbo);
            GL.BufferData(BufferTarget.ArrayBuffer, SkyboxVertices.Length * sizeof(float), SkyboxVertices, BufferUsageHint.StaticDraw);
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(SkyboxShaders.GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.UseProgram(SkyboxShaders.ProgramId);
            SkyboxShaders.SetUniform("skybox", 0);
            List<string> faces = new List<string>()
            {
                "_Resources/Textures/Skybox/metal.png",
                "_Resources/Textures/Skybox/metal.png",
                "_Resources/Textures/Skybox/metal.png",
                "_Resources/Textures/Skybox/metal.png",
                "_Resources/Textures/Skybox/metal.png",
                "_Resources/Textures/Skybox/metal.png",
                "_Resources/Textures/Skybox/metal.png",
                "_Resources/Textures/Skybox/metal.png"
            };

            _cubeMapTexture = LoadCubemap(faces);
            GL.Enable(EnableCap.ProgramPointSize);
        }

        public void DrawSkybox()
        {
            GL.DepthFunc(DepthFunction.Lequal);
            GL.UseProgram(SkyboxShaders.ProgramId);
            SkyboxShaders.SetUniform("view", View);
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView((float)Math.PI * (45f / 180f), AspectRatio, 0.1f, 100.0f);
            SkyboxShaders.SetUniform("projection", projection);
            GL.BindVertexArray(_skyboxVao);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.TextureCubeMap, _cubeMapTexture);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 36);
            GL.BindVertexArray(0);
            GL.DepthFunc(DepthFunction.Less);

        }

        private static int LoadCubemap(List<string> faces)
        {
            int textureId = GL.GenTexture();
            GL.BindTexture(TextureTarget.ProxyTextureCubeMap, textureId);


            for (int i = 0; i < faces.Count; i++)
            {
                Bitmap bitmap = new Bitmap(faces[i]);
                BitmapData bmpData = bitmap.LockBits(
                    new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                OpenTK.Graphics.OpenGL4.GL.TexImage2D(
                    OpenTK.Graphics.OpenGL4.TextureTarget.TextureCubeMapPositiveX + i, 0,
                    OpenTK.Graphics.OpenGL4.PixelInternalFormat.Rgb,
                    bitmap.Width, bitmap.Height, 0,
                    PixelFormat.Bgr, OpenTK.Graphics.OpenGL4.PixelType.UnsignedByte, bmpData.Scan0);

                bitmap.UnlockBits(bmpData);

            }
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);

            return textureId;
        }
    }
}
