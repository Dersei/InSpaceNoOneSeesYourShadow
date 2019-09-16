using System;
using System.Collections.Generic;
using System.Linq;
using ButtonState = OpenTK.Input.ButtonState;
using InSpaceNoOneSeesYourShadow.Helpers;
using InSpaceNoOneSeesYourShadow.Helpers.Cameras;
using InSpaceNoOneSeesYourShadow.Objects3D;
using InSpaceNoOneSeesYourShadow.Objects3D.Shapes;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace InSpaceNoOneSeesYourShadow.Logic
{
    internal class Game
    {
        private Matrix4 _view = Matrix4.Identity;
        public GameCamera Camera { get; } = new GameCamera();
        private readonly Light _directionalLight = new Light(-Vector3.UnitZ, new Vector3(1f, 0.5f, 0.5f));
        private readonly Light _spotLight1 = new Light(new Vector3(1, 5, 1), new Vector3(1f, 1f, 1f));
        private readonly Light _spotLight2 = new Light(new Vector3(5, 10, 0), new Vector3(0.8f, 0.8f, 0f));
        private readonly Light _pointLight = new Light(new Vector3(1, 5, 1), new Vector3(1f, 0f, 0f));
        private readonly Random _random = new Random();
        private readonly List<ObjVolume> _objects = new List<ObjVolume>();
        private readonly List<ObjVolume> _enemies = new List<ObjVolume>();
        private readonly List<ObjVolume> _projectiles = new List<ObjVolume>();
        private readonly List<ObjVolume> _activeEnemyProjectiles = new List<ObjVolume>();
        private readonly List<ObjVolume> _cachedEnemyProjectiles = new List<ObjVolume>();
        private readonly Dictionary<string, int> _textures = new Dictionary<string, int>();
        private readonly Dictionary<string, ShaderProgram> _shaders = new Dictionary<string, ShaderProgram>();
        private string _activeShader = "light";

        private int _score;
        private int _health = 5;

        private DateTime _oldTime;
        public void Update()
        {
            _time += (DateTime.Now - _oldTime).Milliseconds / 1000f;

            // Update model view matrices
            foreach (var v in _objects)
            {
                if (v.ShouldNotRender)
                {
                    continue;
                }
                v.Update(_time);
                v.CalculateModelMatrix();
                v.UpdateMatrices(Camera.GetViewMatrix() * Matrix4.CreateOrthographic(100, 100, 1.0f, 1000.0f));
            }

            _oldTime = DateTime.Now;
            _view = Camera.GetViewMatrix();
        }

        public void Draw()
        {
            CheckCollisionsWithEnemy();
            CheckCollisionsWithPlayer();

            //GL.Enable(EnableCap.Blend);
            //GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.DstAlpha);
            // _shaders[_activeShader].EnableVertexAttributesArrays();
            // _directionalLight.Position += new Vector3((float)Math.Sin(_time));
            SpawnEnemyProjectiles();
            foreach (var v in _objects)
            {
                if (v.ShouldNotRender)
                {
                    continue;
                }
                DrawVolume(v);
            }

            // _shaders[_activeShader].DisableVertexAttributesArrays();

            GL.Flush();
        }

        public void SpawnEnemyProjectiles()
        {
            var chance = _random.Next(0, 100);
            if (chance > 5 || _activeEnemyProjectiles.Count > 8)
            {
                return;
            }
            var temp = _enemies[_random.Next(0, _enemies.Count)];
            var arrow = _cachedEnemyProjectiles.Except(_activeEnemyProjectiles).FirstOrDefault();
            if (arrow is null) return;
            arrow.Position = temp.Position + new Vector3(0,1,0);
            arrow.PositionModifier = f => arrow.Position - new Vector3(0, 0.2f, 0);
            //ObjVolume arrow = ObjVolume.LoadFromFile("_Resources/Models/cone.obj");
            //arrow.TextureId = _textures["sun.png"];
            //arrow.Position = temp.Position;// + new Vector3(0,1,0);
            //arrow.Rotation = new Vector3(-MathHelper.PiOver2, -MathHelper.PiOver2, -MathHelper.PiOver2);
            //arrow.Material = new Material(new Vector3(0.15f), new Vector3(1), new Vector3(0.2f));
            //arrow.PositionModifier = f => arrow.Position - new Vector3(0, 0.2f, 0);
            //arrow.ScaleModifier = f => new Vector3(1f, 1f, 1f);
            //arrow.VolumeShader = _shaders["light"];
            //arrow.Bind();
            _objects.Add(arrow);
            _activeEnemyProjectiles.Add(arrow);
            //_projectiles.Add(arrow);
        }

        private static bool CheckCollision(Vector2 first, Vector2 second)
        {
            return Math.Abs(first.X - second.X) < 5 && Math.Abs(first.Y - second.Y) < 5;
        }

        public void ClearCollections()
        {
            _projectiles.RemoveAll(p => p.ShouldNotRender);
            _activeEnemyProjectiles.RemoveAll(ep => ep.ShouldNotRender);
            _activeEnemyProjectiles.RemoveAll(ep => ep.Position.Y < -50);
            _enemies.RemoveAll(e => e.ShouldNotRender);
        }

        public void CheckCollisionsWithPlayer()
        {
            if (_activeEnemyProjectiles.Count == 0) return;
            foreach (var projectile in _activeEnemyProjectiles.Where(p => !p.ShouldNotRender))
            {

                if (CheckCollision(projectile.Position.Xy, _playerShip.Position.Xy))
                {
                    projectile.ShouldNotRender = true;
                    _health--;
                    //CheckIfDead();
                }
            }
            ClearCollections();
            //CheckIfDead();
        }

        public void CheckCollisionsWithEnemy()
        {
            if (_projectiles.Count == 0) return;
            foreach (var projectile in _projectiles.Where(p => !p.ShouldNotRender))
            {
                foreach (var enemy in _enemies.Where(e => !e.ShouldNotRender))
                {
                    if (CheckCollision(projectile.Position.Xy, enemy.Position.Xy))
                    {
                        enemy.ShouldNotRender = true;
                        projectile.ShouldNotRender = true;
                        _score++;
                    }
                }
            }
            ClearCollections();
            // CheckIfWin();
        }

        public void InitProgram()
        {
            // Load shaders from file
            _shaders.Add("light", new ShaderProgram("_Resources/Shaders/vs_lit.glsl", "_Resources/Shaders/fs_lit.glsl", true));
            _shaders.Add("PBR", new ShaderProgram("_Resources/Shaders/vs_lit.glsl", "_Resources/Shaders/PBR.glsl", true));
            _activeShader = "PBR";
            _textures.Add("sun.png", ImageLoader.LoadImage("_Resources/Textures/sun.png"));
            _textures.Add("ship2_diffuse.bmp", ImageLoader.LoadImage("_Resources/Textures/ship2_diffuse.bmp"));
            _textures.Add("galaxy.png", ImageLoader.LoadImage("_Resources/Textures/galaxy.png"));
            _textures.Add("dread_ship_t.png", ImageLoader.LoadImage("_Resources/Textures/dread_ship_t.png"));
            CreateScene();

            Camera.Position = new Vector3(0f, 0f, -50f);
        }

        private ObjVolume CreateEnemyProjectile()
        {
            ObjVolume arrow = ObjVolume.LoadFromFile("_Resources/Models/cone.obj");
            arrow.TextureId = _textures["sun.png"];
            arrow.Position = Vector3.Zero;
            arrow.Rotation = new Vector3(-MathHelper.PiOver2, -MathHelper.PiOver2, -MathHelper.PiOver2);
            arrow.Material = new Material(new Vector3(0.15f), new Vector3(1), new Vector3(0.2f));
            arrow.PositionModifier = f => arrow.Position;
            arrow.ScaleModifier = f => new Vector3(1f, 1f, 1f);
            arrow.VolumeShader = _shaders["light"];
            arrow.Bind();
            return arrow;
        }

        private ObjVolume _playerShip;
        private float _time;

        private void CreateScene()
        {

            ObjVolume cubePlane = ObjVolume.LoadFromFile("_Resources/Models/simple_cube.obj");
            cubePlane.TextureId = _textures["galaxy.png"];
            cubePlane.Position += new Vector3(0f, 0f, 0f);
            cubePlane.Rotation = new Vector3(MathHelper.PiOver2, 0, 0f);
            cubePlane.Material = new Material(new Vector3(0.15f), new Vector3(1), new Vector3(0.2f));
            //cubePlane.PositionModifier = f => new Vector3(2 * (float)Math.Sin(MathHelper.DegreesToRadians(f * 80 % 360)), 0f, 2 * (float)Math.Cos(MathHelper.DegreesToRadians(f * 80 % 360)));
            cubePlane.ScaleModifier = f => new Vector3(50f, 0.1f, 50f);
            cubePlane.PbrValues = new ObjVolume.PBRValues
            {
                AO = 0.5f,
                Metallic = 0.5f,
                ReflectionStrength = 0.5f,
                Refraction = 4f,
                Roughness = 0.5f
            };
            cubePlane.VolumeShader = _shaders[_activeShader];
            cubePlane.Bind();
            _objects.Add(cubePlane);

            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 6; i++)
                {
                    ObjVolume ship = ObjVolume.LoadFromFile("_Resources/Models/ship_dread_t.obj");
                    ship.TextureId = _textures["dread_ship_t.png"];
                    ship.Position += new Vector3(40f - 15 * i, 20f - 10 * j, 0f);
                    ship.PositionModifier = f => ship.Position + new Vector3((float)Math.Sin(_time) / 100f, -_time / 1000f, 0);
                    ship.Rotation = new Vector3(MathHelper.Pi, 0, -MathHelper.PiOver2);
                    ship.Material = new Material(new Vector3(0.5f), new Vector3(1), new Vector3(0.8f));
                    //cubePlane.PositionModifier = f => new Vector3(2 * (float)Math.Sin(MathHelper.DegreesToRadians(f * 80 % 360)), 0f, 2 * (float)Math.Cos(MathHelper.DegreesToRadians(f * 80 % 360)));
                    ship.ScaleModifier = f => new Vector3(0.005f, 0.005f, 0.005f);
                    ship.PbrValues = new ObjVolume.PBRValues
                    {
                        AO = 0.5f,
                        Metallic = 0.5f,
                        ReflectionStrength = (float)_random.NextDouble(),
                        Refraction = 0f,
                        Roughness = 0.5f
                    };
                    ship.VolumeShader = _shaders["PBR"];
                    ship.Bind();
                    _objects.Add(ship);
                    _enemies.Add(ship);
                }
            }


            ObjVolume playerShip = ObjVolume.LoadFromFile("_Resources/Models/ship2.obj");
            playerShip.Position += new Vector3(0f, -40f, -10f);
            playerShip.Rotation = new Vector3(-MathHelper.PiOver2, 0, 0);
            playerShip.Scale = new Vector3(0.05f, 0.05f, 0.05f);
            //cat.PositionModifier = f => new Vector3(10, 4, 0);
            //cat.Material = _materials["ship2mat"];
            playerShip.Material = new Material(new Vector3(0.5f), new Vector3(1), new Vector3(0.8f));
            playerShip.TextureId = _textures["ship2_diffuse.bmp"];
            playerShip.PbrValues = new ObjVolume.PBRValues
            {
                AO = 1f,
                Metallic = 0.5f,
                ReflectionStrength = 0f,
                Refraction = 0f,
                Roughness = 0f
            };
            playerShip.VolumeShader = _shaders["light"];
            playerShip.Bind();
            _playerShip = playerShip;
            _objects.Add(playerShip);

            for (var i = 0; i < 8; i++)
            {
                _cachedEnemyProjectiles.Add(CreateEnemyProjectile());
            }
        }

        public void DrawVolume(ObjVolume volume)
        {
            volume.VolumeShader.EnableVertexAttributesArrays();
            GL.UseProgram(volume.VolumeShader.ProgramId);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, volume.TextureId);
            volume.VolumeShader.SetUniform("modelview", volume.ModelViewProjectionMatrix);
            volume.VolumeShader.SetAttribute("vColor", volume.GetColorData());
            // volume.VolumeShader.SetUniform("maintexture", volume.TextureId);
            volume.VolumeShader.SetUniform("view", _view);
            volume.VolumeShader.SetUniform("camPos", Camera.Position);
            volume.VolumeShader.SetUniform("model", volume.ModelMatrix);
            volume.VolumeShader.SetUniform("material_ambient", volume.Material.DiffuseColor);
            volume.VolumeShader.SetUniform("material_diffuse", volume.Material.DiffuseColor);
            volume.VolumeShader.SetUniform("material_specular", volume.Material.SpecularColor);
            volume.VolumeShader.SetUniform("material_specExponent", volume.Material.SpecularExponent);
            volume.VolumeShader.SetUniform("light_position", _directionalLight.Position);
            volume.VolumeShader.SetUniform("light_color", _directionalLight.Color);
            volume.VolumeShader.SetUniform("light_diffuseIntensity", _directionalLight.DiffuseIntensity);
            volume.VolumeShader.SetUniform("light_ambientIntensity", _directionalLight.AmbientIntensity);
            volume.VolumeShader.SetUniform("time", _time);

            if (volume.VolumeShader == _shaders["PBR"])
            {
                volume.VolumeShader.SetUniform("ao", volume.PbrValues.AO);
                volume.VolumeShader.SetUniform("metallic", volume.PbrValues.Metallic);
                volume.VolumeShader.SetUniform("roughness", volume.PbrValues.Roughness);
                volume.VolumeShader.SetUniform("reflectionStrength", volume.PbrValues.ReflectionStrength);
                volume.VolumeShader.SetUniform("refraction", volume.PbrValues.Refraction);
                volume.VolumeShader.SetUniform("dirLight.direction", _directionalLight.Position);
                volume.VolumeShader.SetUniform("dirLight.color", _directionalLight.Color);
                volume.VolumeShader.SetUniform("dirLight.lightStrength", 10f);
                volume.VolumeShader.SetUniform("pointLight.position", _pointLight.Position);
                volume.VolumeShader.SetUniform("pointLight.color", _pointLight.Color);
                volume.VolumeShader.SetUniform("spotLight[0].cutOff", (float)Math.Cos(MathHelper.RadiansToDegrees(10f)));
                volume.VolumeShader.SetUniform("spotLight[0].outerCutOff", (float)Math.Cos(MathHelper.RadiansToDegrees(80f)));
                //volume.VolumeShader.SetVec3("spotLight[0].color", _spotLight1.Color);
                volume.VolumeShader.SetUniform("spotLight[0].color", new Vector3(0, 1, 0));
                //volume.VolumeShader.SetVec3("spotLight[0].position", _spotLight1.Position);
                volume.VolumeShader.SetUniform("spotLight[0].position", _playerShip.Position);
                volume.VolumeShader.SetUniform("spotLight[0].direction", new Vector3(0, 1, 0));
                volume.VolumeShader.SetUniform("spotLight[1].cutOff", (float)Math.Cos(MathHelper.RadiansToDegrees(0f)));
                volume.VolumeShader.SetUniform("spotLight[1].outerCutOff", (float)Math.Cos(MathHelper.RadiansToDegrees(0f)));
                volume.VolumeShader.SetUniform("spotLight[1].color", _spotLight2.Color);
                volume.VolumeShader.SetUniform("spotLight[1].position", _spotLight2.Position);
                volume.VolumeShader.SetUniform("spotLight[1].direction", new Vector3(0, -1, 0));
            }

            GL.BindVertexArray(volume.VAO);
            GL.DrawElements(PrimitiveType.Triangles, volume.IndicesCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);

            //foreach (var child in volume.Children)
            //{
            //    GL.PolygonMode(MaterialFace.FrontAndBack, _polygonMode);
            //    DrawVolume(child, indexAt);
            //}
            volume.VolumeShader.DisableVertexAttributesArrays();

        }

        private void ShootPlayerProjectile()
        {
            var arrow = ObjVolume.LoadFromFile("_Resources/Models/arrow.obj");
            arrow.TextureId = _textures["sun.png"];
            arrow.Position = _playerShip.Position;// + new Vector3(0,1,0);
            arrow.Rotation = new Vector3(-MathHelper.PiOver2, 0, 0);
            arrow.Material = new Material(new Vector3(0.15f), new Vector3(1), new Vector3(0.2f));
            arrow.PositionModifier = f => arrow.Position + new Vector3(0, 0.2f, 0);
            arrow.ScaleModifier = f => new Vector3(1f, 1f, 1f);
            arrow.VolumeShader = _shaders["light"];
            arrow.Bind();
            _objects.Add(arrow);
            _projectiles.Add(arrow);
        }


        private bool _wasSpacePressed;

        public void ProcessInput()
        {
            if (Keyboard.GetState().IsKeyDown(Key.A))
            {
                if (_playerShip.Position.X > 45)
                    return;
                _playerShip.Position += new Vector3(0.5f, 0, 0);
            }
            if (Keyboard.GetState().IsKeyDown(Key.D))
            {
                if (_playerShip.Position.X < -45)
                    return;
                _playerShip.Position += new Vector3(-0.5f, 0, 0);
            }
            if (Keyboard.GetState().IsKeyDown(Key.Space) && !_wasSpacePressed)
            {
                ShootPlayerProjectile();
                _wasSpacePressed = true;
                return;
            }

            _wasSpacePressed = !Keyboard.GetState().IsKeyUp(Key.Space);
        }
    }
}
