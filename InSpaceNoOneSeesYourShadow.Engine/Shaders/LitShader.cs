namespace InSpaceNoOneSeesYourShadow.Engine.Shaders
{
    public class LitShader : ShaderProgram
    {
        public LitShader() : base("_Resources/Shaders/vertex_lit.glsl", "_Resources/Shaders/fragment_lit.glsl")
        {
        }
    }
}
