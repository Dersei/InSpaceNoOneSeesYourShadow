using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using InSpaceNoOneSeesYourShadow.Engine.Helpers;

namespace InSpaceNoOneSeesYourShadow.Engine.ContentManagement
{
    public class Texture2DLoader
    {
        private static readonly Dictionary<string, Texture2D> Cache = new();

        private static bool CheckIfCached(string name, [NotNullWhen(true)]out Texture2D? obj)
        {
            return Cache.TryGetValue(name, out obj);
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
