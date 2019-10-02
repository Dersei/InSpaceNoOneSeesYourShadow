using System.Runtime.InteropServices;
using OpenTK;

namespace InSpaceNoOneSeesYourShadow.Engine.Objects3D
{
    public partial class Model
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private readonly struct Vertex
        {
            // ReSharper disable PrivateFieldCanBeConvertedToLocalVariable
            private readonly Vector3 Position;
            private readonly Vector3 Normal;
            private readonly Vector2 TexCoords;
            // ReSharper restore PrivateFieldCanBeConvertedToLocalVariable

            public Vertex(Vector3 position, Vector3 normal, Vector2 texCoords)
            {
                Position = position;
                Normal = normal;
                TexCoords = texCoords;
            }
        }

        public struct PBRValues
        {
            public float AO { get; set; }
            public float Metallic { get; set; }
            public float Roughness { get; set; }
            public float ReflectionStrength { get; set; }
            public float Refraction { get; set; }
        }

        public readonly struct FaceVertex
        {
            public readonly Vector3 Position;
            public readonly Vector3 Normal;
            public readonly Vector2 TextureCoords;

            public FaceVertex(Vector3 pos, Vector3 norm, Vector2 texCoords)
            {
                Position = pos;
                Normal = norm;
                TextureCoords = texCoords;
            }
        }

    }
}
