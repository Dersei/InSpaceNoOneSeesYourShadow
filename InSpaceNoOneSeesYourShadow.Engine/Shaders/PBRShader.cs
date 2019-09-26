using System;
using OpenTK;

namespace InSpaceNoOneSeesYourShadow.Engine.Shaders
{
    public class PBRShader : ShaderProgram
    {
        public PBRShader() : base("_Resources/Shaders/vertex_lit.glsl", "_Resources/Shaders/fragment_PBR.glsl", true)
        {
        }

        public override void Draw()
        {
            base.Draw();
            Model.VolumeShader.SetUniform("ao", Model.PbrValues.AO);
            Model.VolumeShader.SetUniform("metallic", Model.PbrValues.Metallic);
            Model.VolumeShader.SetUniform("roughness", Model.PbrValues.Roughness);
            Model.VolumeShader.SetUniform("reflectionStrength", Model.PbrValues.ReflectionStrength);
            Model.VolumeShader.SetUniform("refraction", Model.PbrValues.Refraction);
            Model.VolumeShader.SetUniform("dirLight.direction", GameManager.DirectionalLight.Position);
            Model.VolumeShader.SetUniform("dirLight.color", GameManager.DirectionalLight.Color);
            Model.VolumeShader.SetUniform("dirLight.lightStrength", 10f);
            Model.VolumeShader.SetUniform("pointLight.position", GameManager.PointLight.Position);
            Model.VolumeShader.SetUniform("pointLight.color", GameManager.PointLight.Color);
            Model.VolumeShader.SetUniform("spotLight[0].cutOff", (float)Math.Cos(MathHelper.RadiansToDegrees(10f)));
            Model.VolumeShader.SetUniform("spotLight[0].outerCutOff", (float)Math.Cos(MathHelper.RadiansToDegrees(80f)));
            //volume.VolumeShader.SetVec3("spotLight[0].color", _spotLight1.Color);
            Model.VolumeShader.SetUniform("spotLight[0].color", new Vector3(0, 1, 0));
            //volume.VolumeShader.SetVec3("spotLight[0].position", _spotLight1.Position);
            Model.VolumeShader.SetUniform("spotLight[0].position", GameManager.PlayerPosition);
            Model.VolumeShader.SetUniform("spotLight[0].direction", new Vector3(0, 1, 0));
            Model.VolumeShader.SetUniform("spotLight[1].cutOff", (float)Math.Cos(MathHelper.RadiansToDegrees(0f)));
            Model.VolumeShader.SetUniform("spotLight[1].outerCutOff", (float)Math.Cos(MathHelper.RadiansToDegrees(0f)));
            Model.VolumeShader.SetUniform("spotLight[1].color", GameManager.Spotlight.Color);
            Model.VolumeShader.SetUniform("spotLight[1].position", GameManager.Spotlight.Position);
            Model.VolumeShader.SetUniform("spotLight[1].direction", new Vector3(0, -1, 0));
        }
    }
}
