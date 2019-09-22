using System;
using System.Collections.Generic;
using System.Linq;
using InSpaceNoOneSeesYourShadow.Engine;
using InSpaceNoOneSeesYourShadow.Engine.Cameras;
using InSpaceNoOneSeesYourShadow.Engine.ContentManagement;
using InSpaceNoOneSeesYourShadow.Engine.Core;
using InSpaceNoOneSeesYourShadow.Engine.Extensions;
using InSpaceNoOneSeesYourShadow.Engine.Helpers;
using InSpaceNoOneSeesYourShadow.Engine.Objects3D;
using InSpaceNoOneSeesYourShadow.Engine.Objects3D.Shapes;
using InSpaceNoOneSeesYourShadow.Engine.Utils;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace InSpaceNoOneSeesYourShadow.Logic
{
    internal class Game
    {
        public GameCamera Camera { get; } = new GameCamera();
        private readonly Light _directionalLight = new Light(-Vector3.UnitZ, new Vector3(1f, 0.5f, 0.5f));
        private readonly Light _spotLight1 = new Light(new Vector3(1, 5, 1), new Vector3(1f, 1f, 1f));
        private readonly Light _spotLight2 = new Light(new Vector3(5, 10, 0), new Vector3(0.8f, 0.8f, 0f));
        private readonly Light _pointLight = new Light(new Vector3(1, 5, 1), new Vector3(1f, 0f, 0f));
        private readonly List<GameObject> _objects = new List<GameObject>();
        private readonly List<GameObject> _enemies = new List<GameObject>();
        private readonly List<GameObject> _projectiles = new List<GameObject>();
        private readonly List<GameObject> _activeEnemyProjectiles = new List<GameObject>();
        private readonly List<GameObject> _cachedEnemyProjectiles = new List<GameObject>();
        private readonly Dictionary<string, int> _textures = new Dictionary<string, int>();
        private readonly Dictionary<string, ShaderProgram> _shaders = new Dictionary<string, ShaderProgram>();
        private string _activeShader = "light";

        private int _score;
        private int _health = 5;

        private DateTime _oldTime;
        public void Update()
        {
            GameManager.Time = _time += (DateTime.Now - _oldTime).Milliseconds / 1000f;

            // Update model view matrices
            foreach (var v in _objects)
            {
                v.Update(_time);
            }

            _oldTime = DateTime.Now;
            GameManager.PlayerPosition = _playerShip.Transform.Position;
        }

        public void Draw()
        {
            CheckCollisionsWithEnemy();
            CheckCollisionsWithPlayer();
            SpawnEnemyProjectiles();
            foreach (var v in _objects)
            {
                v.Draw();
            }

            GL.Flush();
        }

        public void SpawnEnemyProjectiles()
        {
            var chance = Basic.Random.Next(0, 100);
            if (chance > 5 || _activeEnemyProjectiles.Count > 8)
            {
                return;
            }
            var temp = _enemies.RandomItem();
            var arrow = _cachedEnemyProjectiles.Except(_activeEnemyProjectiles).FirstOrDefault();
            if (arrow is null) return;
            arrow.Transform.Position = temp.Transform.Position + new Vector3(0,1,0);
            arrow.Transform.PositionModifier = f => arrow.Transform.Position - new Vector3(0, 0.2f, 0);
            _objects.Add(arrow);
            _activeEnemyProjectiles.Add(arrow);
        }

        private static bool CheckCollision(Vector2 first, Vector2 second)
        {
            return Math.Abs(first.X - second.X) < 5 && Math.Abs(first.Y - second.Y) < 5;
        }

        public void ClearCollections()
        {
            _projectiles.RemoveAll(p => p.ShouldNotRender);
            _activeEnemyProjectiles.RemoveAll(ep => ep.ShouldNotRender);
            _activeEnemyProjectiles.RemoveAll(ep => ep.Transform.Position.Y < -50);
            _enemies.RemoveAll(e => e.ShouldNotRender);
        }

        public void CheckCollisionsWithPlayer()
        {
            if (_activeEnemyProjectiles.Count == 0) return;
            foreach (var projectile in _activeEnemyProjectiles.Where(p => !p.ShouldNotRender))
            {

                if (CheckCollision(projectile.Transform.Position.Xy, _playerShip.Transform.Position.Xy))
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
                    if (CheckCollision(projectile.Transform.Position.Xy, enemy.Transform.Position.Xy))
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
            GameManager.Shaders = _shaders;
            _activeShader = "PBR";
            _textures.Add("sun.png", ImageLoader.LoadImage("_Resources/Textures/sun.png"));
            _textures.Add("ship2_diffuse.bmp", ImageLoader.LoadImage("_Resources/Textures/ship2_diffuse.bmp"));
            _textures.Add("galaxy.png", ImageLoader.LoadImage("_Resources/Textures/galaxy.png"));
            _textures.Add("dread_ship_t.png", ImageLoader.LoadImage("_Resources/Textures/dread_ship_t.png"));
            CreateScene();
            GameManager.Camera = Camera;
            GameManager.DirectionalLight = _directionalLight;
            GameManager.Spotlight = _spotLight2;
            GameManager.PointLight = _pointLight;
            Camera.Position = new Vector3(0f, 0f, -50f);
            Camera.ProjectionMatrix = Matrix4.CreateOrthographic(100, 100, 1.0f, 1000.0f);
        }

        private GameObject CreateEnemyProjectile()
        {
            GameObject arrow = new GameObject(Vector3.Zero, new Vector3(-MathHelper.PiOver2, -MathHelper.PiOver2, -MathHelper.PiOver2), Vector3.One, ModelLoader.LoadFromFile("_Resources/Models/cone.obj"));
            arrow.Model.TextureId = _textures["sun.png"];
            arrow.Model.Material = new Material(new Vector3(0.15f), new Vector3(1), new Vector3(0.2f));
            arrow.Transform.PositionModifier = f => arrow.Transform.Position;
            arrow.Transform.ScaleModifier = f => new Vector3(1f, 1f, 1f);
            arrow.Model.VolumeShader = _shaders["light"];
            arrow.Model.Bind();
            return arrow;
        }

        private GameObject _playerShip;
        private float _time;

        private void CreateScene()
        {
            GameObject cubePlane = new GameObject(new Vector3(0f, 0f, 0f), new Vector3(MathHelper.PiOver2, 0, 0f), Vector3.One, ModelLoader.LoadFromFile("_Resources/Models/simple_cube.obj"));
            cubePlane.Model.TextureId = _textures["galaxy.png"];
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
            cubePlane.Model.VolumeShader = _shaders[_activeShader];
            cubePlane.Model.Bind();
            _objects.Add(cubePlane);

            GameObject playerShip = new GameObject(new Vector3(0f, -40f, -10f), new Vector3(-MathHelper.PiOver2, 0, 0), new Vector3(0.05f, 0.05f, 0.05f), ModelLoader.LoadFromFile("_Resources/Models/ship2.obj"));
            playerShip.Model.Material = new Material(new Vector3(1), new Vector3(1), new Vector3(0.8f));
            playerShip.Model.TextureId = _textures["ship2_diffuse.bmp"];
            playerShip.Model.PbrValues = new Model.PBRValues
            {
                AO = 1f,
                Metallic = 0.5f,
                ReflectionStrength = 0f,
                Refraction = 0f,
                Roughness = 0f
            };
            playerShip.Model.VolumeShader = _shaders["light"];
            playerShip.Model.Bind();
            _playerShip = playerShip;
            _objects.Add(playerShip);

            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 6; i++)
                {
                    GameObject ship = new GameObject(new Vector3(40f - 15 * i, 20f - 10 * j, 0f), new Vector3(MathHelper.Pi, 0, -MathHelper.PiOver2), Vector3.One, ModelLoader.LoadFromFile("_Resources/Models/ship_dread_t.obj"));
                    ship.Model.TextureId = _textures["dread_ship_t.png"];
                    //ship.Transform.Position += new Vector3(40f - 15 * i, 20f - 10 * j, 0f);
                    ship.Transform.PositionModifier = f => ship.Transform.Position + new Vector3((float)Math.Sin(_time) / 100f, -_time / 1000f, 0);
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
                    ship.Model.VolumeShader = _shaders["PBR"];
                    ship.Model.Bind();
                    _objects.Add(ship);
                    _enemies.Add(ship);
                }
            }

            for (var i = 0; i < 8; i++)
            {
                _cachedEnemyProjectiles.Add(CreateEnemyProjectile());
            }
        }


        private void ShootPlayerProjectile()
        {
            GameObject arrow = new GameObject(_playerShip.Transform.Position, new Vector3(-MathHelper.PiOver2, 0, 0), Vector3.One, ModelLoader.LoadFromFile("_Resources/Models/arrow.obj"));

            arrow.Model.TextureId = _textures["sun.png"];
            arrow.Model.Material = new Material(new Vector3(0.15f), new Vector3(1), new Vector3(0.2f));
            arrow.Transform.PositionModifier = f => arrow.Transform.Position + new Vector3(0, 0.2f, 0);
            arrow.Transform.ScaleModifier = f => new Vector3(1f, 1f, 1f);
            arrow.Model.VolumeShader = _shaders["light"];
            arrow.Model.Bind();
            _objects.Add(arrow);
            _projectiles.Add(arrow);
        }


        private bool _wasSpacePressed;

        public void ProcessInput()
        {
            if (Keyboard.GetState().IsKeyDown(Key.A))
            {
                if (_playerShip.Transform.Position.X > 45)
                    return;
                _playerShip.Transform.Position += new Vector3(0.5f, 0, 0);
            }
            if (Keyboard.GetState().IsKeyDown(Key.D))
            {
                if (_playerShip.Transform.Position.X < -45)
                    return;
                _playerShip.Transform.Position += new Vector3(-0.5f, 0, 0);
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
