using OpenTK;

namespace InSpaceNoOneSeesYourShadow.Engine.Helpers
{
    public class Light
    {
        public Light(Vector3 position, Vector3 color, float diffuseIntensity = 1.0f, float ambientIntensity = 1.0f)
        {
            Position = position;
            Color = color;

            DiffuseIntensity = diffuseIntensity;
            AmbientIntensity = ambientIntensity;
        }

        public Vector3 Position;
        public Vector3 Color;
        public float DiffuseIntensity;
        public float AmbientIntensity;
    }
}
