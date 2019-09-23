namespace InSpaceNoOneSeesYourShadow.Engine.Shaders
{
    internal class LitShader : ShaderProgram
    {
        public LitShader() : base("_Resources/Shaders/vs_lit.glsl", "_Resources/Shaders/fs_lit.glsl", true)
        {
        }
    }
}
