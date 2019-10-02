using System;
using InSpaceNoOneSeesYourShadow.Engine;
using InSpaceNoOneSeesYourShadow.Engine.Abstractions;
using InSpaceNoOneSeesYourShadow.Engine.ContentManagement;
using InSpaceNoOneSeesYourShadow.Engine.Core;
using InSpaceNoOneSeesYourShadow.Engine.Objects3D;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace InSpaceNoOneSeesYourShadow.Logic
{
    internal class Level1Scene : Scene
    {
        public override void CreateScene()
        {
            GameObject cubePlane = new GameObject(new Vector3(0f, 0f, 0f), 
                new Vector3(MathHelper.PiOver2, 0, 0f), 
                "_Resources/Models/simple_cube.obj");
            cubePlane.Model.Texture = Texture2DLoader.LoadFromFile("_Resources/Textures/galaxy.png");
            cubePlane.Transform.Position += new Vector3(0f, 0f, 0f);
            cubePlane.Transform.Rotation = new Vector3(MathHelper.PiOver2, 0, 0f);
            cubePlane.Model.Material = new Material(new Vector3(0.5f), new Vector3(1), new Vector3(0.2f));
            //cubePlane.Transform.PositionModifier = f => new Vector3(2 * (float)Math.Sin(MathHelper.DegreesToRadians(f * 80 % 360)), 0f, 2 * (float)Math.Cos(MathHelper.DegreesToRadians(f * 80 % 360)));
            cubePlane.Transform.ScaleModifier = f => new Vector3(50f, 0.1f, 50f);
            cubePlane.Model.PbrValues = new Model.PBRValues
            {
                AO = 0.5f,
                Metallic = 0.5f,
                ReflectionStrength = 0.5f,
                Refraction = 4f,
                Roughness = 0.5f
            };
            cubePlane.Model.VolumeShader = GameManager.Shaders[GameManager.ActiveShader];
            cubePlane.Model.Bind();

            GameObject playerShip = new GameObject(new Vector3(0f, -40f, -10f),
                new Vector3(-MathHelper.PiOver2, 0, 0),
                new Vector3(0.05f, 0.05f, 0.05f), 
                "_Resources/Models/ship2.obj");
            playerShip.Model.Material = new Material(new Vector3(1), new Vector3(1), new Vector3(0.8f));
            playerShip.Model.Texture = Texture2DLoader.LoadFromFile("_Resources/Textures/ship2_diffuse.bmp");
            playerShip.Model.PbrValues = new Model.PBRValues
            {
                AO = 1f,
                Metallic = 0.5f,
                ReflectionStrength = 0f,
                Refraction = 0f,
                Roughness = 0f
            };
            playerShip.Model.VolumeShader = GameManager.Shaders["light"];
            playerShip.Model.Bind();
            playerShip.AddComponent(new PlayerComponent());
            playerShip.AddComponent(new ShooterComponent());

            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 6; i++)
                {
                    GameObject ship = new GameObject(new Vector3(40f - 15 * i, 20f - 10 * j, 0f), new Vector3(MathHelper.Pi, 0, -MathHelper.PiOver2), Vector3.One, ModelLoader.LoadFromFile("_Resources/Models/ship_dread_t.obj"));
                    ship.Model.Texture = Texture2DLoader.LoadFromFile("_Resources/Textures/dread_ship_t.png");
                    //ship.Transform.Position += new Vector3(40f - 15 * i, 20f - 10 * j, 0f);
                    ship.Transform.PositionModifier = f => ship.Transform.Position + new Vector3((float)Math.Sin(GameManager.Time) / 100f, -GameManager.Time / 1000f, 0);
                    ship.Transform.Rotation = new Vector3(MathHelper.Pi, 0, -MathHelper.PiOver2);
                    ship.Model.Material = new Material(new Vector3(0.5f), new Vector3(1), new Vector3(0.8f));
                    //cubePlane.PositionModifier = f => new Vector3(2 * (float)Math.Sin(MathHelper.DegreesToRadians(f * 80 % 360)), 0f, 2 * (float)Math.Cos(MathHelper.DegreesToRadians(f * 80 % 360)));
                    ship.Transform.ScaleModifier = f => new Vector3(0.005f, 0.005f, 0.005f);
                    ship.Model.PbrValues = new Model.PBRValues
                    {
                        AO = 0.5f,
                        Metallic = 0.5f,
                        ReflectionStrength = (float)Basic.Random.NextDouble(),
                        Refraction = 0f,
                        Roughness = 0.5f
                    };
                    ship.Model.VolumeShader = GameManager.Shaders["PBR"];
                    ship.Model.Bind();
                    ship.AddComponent<EnemyComponent>();
                }
            }

            ReorganizeCollections();
            foreach (var gameObject in _gameObjects)
            {
                gameObject.Start();
            }
        }

        public override void Update(float time)
        {
            base.Update(time);
            foreach (var v in _gameObjects)
            {
                v.Update(time);
            }
        }

        public override void Draw()
        {
            foreach (var v in _gameObjects)
            {
                v.Draw();
            }

            GL.Flush();
        }
    }
}
