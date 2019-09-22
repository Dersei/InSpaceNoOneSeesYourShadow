using OpenTK;
using System.Collections.Generic;
using InSpaceNoOneSeesYourShadow.Engine.Cameras;
using InSpaceNoOneSeesYourShadow.Engine.Helpers;

namespace InSpaceNoOneSeesYourShadow
{
    public static class GameManager
    {
        public static Camera Camera;
        public static Light DirectionalLight;
        public static Light Spotlight;
        public static Light PointLight;
        public static float Time;
        public static Dictionary<string, ShaderProgram> Shaders;
        public static Vector3 PlayerPosition;
    }
}
