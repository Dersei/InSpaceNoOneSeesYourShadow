using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace InSpaceNoOneSeesYourShadow.Engine.Utils
{
    public static class ImageLoader
    {

        public static int LoadImage(Bitmap image)
        {
            var texId = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, texId);
            var data = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);

            image.UnlockBits(data);

            GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            return texId;
        }

        public static int LoadImage(string filename)
        {
            try
            {
                var file = new Bitmap(filename);
                return LoadImage(file);
            }
            catch (FileNotFoundException)
            {
                return -1;
            }
        }
    }
}
