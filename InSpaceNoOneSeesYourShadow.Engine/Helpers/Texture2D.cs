using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace InSpaceNoOneSeesYourShadow.Engine.Helpers
{
    public class Texture2D
    {
        public int ID { get; }
        public int Width { get; }
        public int Height { get; }

        public Texture2D(int id, int width, int height)
        {
            ID = id;
            Width = width;
            Height = height;
        }



        public static Texture2D LoadTexture(string path)
        {
            using var bitmap = new Bitmap(path);
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
            return new Texture2D(id, bitmap.Width, bitmap.Height);
        }
    }
}
