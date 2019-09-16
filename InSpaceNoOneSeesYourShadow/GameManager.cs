using InSpaceNoOneSeesYourShadow.Helpers;
using InSpaceNoOneSeesYourShadow.Helpers.Cameras;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace InSpaceNoOneSeesYourShadow
{
    public static class GameManager
    {
        public static GameCamera Camera;
        public static Light DirectionalLight;
        public static Light Spotlight;
        public static Light Pointlight;
        public static float Time;
        public static Dictionary<string, ShaderProgram> Shaders;
        public static Vector3 PlayerPosition;
    }
}
