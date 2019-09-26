using System.Collections.Generic;
using InSpaceNoOneSeesYourShadow.Engine.Abstractions;
using InSpaceNoOneSeesYourShadow.Engine.Cameras;
using InSpaceNoOneSeesYourShadow.Engine.Environment;
using InSpaceNoOneSeesYourShadow.Engine.Helpers;
using InSpaceNoOneSeesYourShadow.Engine.Shaders;
using OpenTK;

namespace InSpaceNoOneSeesYourShadow.Engine
{
    public static class GameManager
    {
        public static Camera Camera;
        public static Light DirectionalLight;
        public static Light Spotlight;
        public static Light PointLight;
        public static float Time;
        public static Dictionary<string, ShaderProgram> Shaders;
        public static Dictionary<string, int> Textures;
        public static string ActiveShader;
        public static Vector3 PlayerPosition;
        public static Scene CurrentScene;
    }
}
