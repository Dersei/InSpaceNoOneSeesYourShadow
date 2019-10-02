using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using InSpaceNoOneSeesYourShadow.Engine.Helpers;
using OpenTK.Graphics.OpenGL4;
using PixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace InSpaceNoOneSeesYourShadow.Engine.ContentManagement
{
    public class Texture2DLoader
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
            var texture = Texture2D.LoadTexture(filename);

            Cache.Add(filename, texture);

            return texture;
        }
    }
}
