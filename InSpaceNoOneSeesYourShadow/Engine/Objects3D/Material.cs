using OpenTK;

namespace InSpaceNoOneSeesYourShadow.Engine.Objects3D
{
    /// <summary>
    /// Stores information about a material applied to a <c>Volume</c>
    /// </summary>
    public class Material
    {
        public Vector3 AmbientColor;
        public Vector3 DiffuseColor;
        public Vector3 SpecularColor;
        public float SpecularExponent = 1;
        public float Opacity = 1.0f;

        public string AmbientMap = "";
        public string DiffuseMap = "";
        public string SpecularMap = "";
        public string OpacityMap = "";
        public string NormalMap = "";

        public Material()
        {
        }

        public Material(Vector3 ambient, Vector3 diffuse, Vector3 specular, float specularExponent = 1.0f, float opacity = 1.0f)
        {
            AmbientColor = ambient;
            DiffuseColor = diffuse;
            SpecularColor = specular;
            SpecularExponent = specularExponent;
            Opacity = opacity;
        }
    }
}
