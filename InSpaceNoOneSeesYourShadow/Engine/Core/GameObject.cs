using System;
using InSpaceNoOneSeesYourShadow.Engine.Objects3D.Shapes;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace InSpaceNoOneSeesYourShadow.Engine.Core
{
    public class GameObject
    {
        public Transform Transform;
        public ObjVolume Model;
        public bool ShouldNotRender;

        public GameObject(Vector3 position, Vector3 rotation, Vector3 scale, ObjVolume model)
        {
            Transform = new Transform(position, rotation, scale);
            Model = model;
        }

        public void Update(float value)
        {
            if (ShouldNotRender) return;
            Model.CalculateModelMatrix(Transform);
            //Model.UpdateMatrices(GameManager.Camera.GetViewMatrix() * Matrix4.CreateOrthographic(100, 100, 1.0f, 1000.0f));

            Transform.Update(value);
        }

        public void Draw()
        {
            if (ShouldNotRender) return;
            Model.VolumeShader.EnableVertexAttributesArrays();
            GL.UseProgram(Model.VolumeShader.ProgramId);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, Model.TextureId);
            Model.VolumeShader.SetUniform("modelview", Transform.ModelMatrix * GameManager.Camera.ViewProjectionMatrix);
            Model.VolumeShader.SetAttribute("vColor", Model.GetColorData());
            // volume.VolumeShader.SetUniform("maintexture", volume.TextureId);
            Model.VolumeShader.SetUniform("view", GameManager.Camera.ViewMatrix);
            Model.VolumeShader.SetUniform("camPos", GameManager.Camera.Position);
            Model.VolumeShader.SetUniform("model", Transform.ModelMatrix);
            Model.VolumeShader.SetUniform("material_ambient", Model.Material.AmbientColor);
            Model.VolumeShader.SetUniform("material_diffuse", Model.Material.DiffuseColor);
            Model.VolumeShader.SetUniform("material_specular", Model.Material.SpecularColor);
            Model.VolumeShader.SetUniform("material_specExponent", Model.Material.SpecularExponent);
            Model.VolumeShader.SetUniform("light_position", GameManager.DirectionalLight.Position);
            Model.VolumeShader.SetUniform("light_color", GameManager.DirectionalLight.Color);
            Model.VolumeShader.SetUniform("light_diffuseIntensity", GameManager.DirectionalLight.DiffuseIntensity);
            Model.VolumeShader.SetUniform("light_ambientIntensity", GameManager.DirectionalLight.AmbientIntensity);
            Model.VolumeShader.SetUniform("time", GameManager.Time);

            if (Model.VolumeShader == GameManager.Shaders["PBR"])
            {
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

            GL.BindVertexArray(Model.VAO);
            GL.DrawElements(PrimitiveType.Triangles, Model.IndicesCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);

            //foreach (var child in volume.Children)
            //{
            //    GL.PolygonMode(MaterialFace.FrontAndBack, _polygonMode);
            //    DrawVolume(child, indexAt);
            //}
            Model.VolumeShader.DisableVertexAttributesArrays();
        }

    }
}
