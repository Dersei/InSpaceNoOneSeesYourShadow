using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using InSpaceNoOneSeesYourShadow.Engine.Helpers;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace InSpaceNoOneSeesYourShadow.Engine.ContentManagement
{
    internal class Texture2DLoader
    {
        private static readonly Dictionary<string, Texture2D> Cache = new Dictionary<string, Texture2D>();

        private static bool CheckIfCached(string name, out Texture2D obj)
        {
            if (Cache.ContainsKey(name))
            {
                obj = Cache[name];
                return true;
            }

            obj = default;
            return false;
        }
        public static Texture2D LoadFromFile(string filename)
        {
            if (CheckIfCached(filename, out var result))
            {
                return result;
            }
            using var bitmap = new Bitmap(filename);
            var id = GL.GenTexture();
            var bmpData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            GL.BindTexture(TextureTarget.Texture2D, id);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bitmap.Width, bitmap.Height, 0,
                PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            bitmap.UnlockBits(bmpData);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            var texture = new Texture2D(id, bitmap.Width, bitmap.Height);

            Cache.Add(filename, texture);

            return texture;
        }
    }
}
