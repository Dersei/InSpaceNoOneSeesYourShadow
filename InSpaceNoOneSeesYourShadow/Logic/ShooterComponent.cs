using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using InSpaceNoOneSeesYourShadow.Engine;
using InSpaceNoOneSeesYourShadow.Engine.ContentManagement;
using InSpaceNoOneSeesYourShadow.Engine.Core;
using InSpaceNoOneSeesYourShadow.Engine.Objects3D;
using OpenTK.Mathematics;

namespace InSpaceNoOneSeesYourShadow.Logic
{
    internal class ShooterComponent : Component
    {
        private readonly List<GameObject> _projectiles = new();
        private List<GameObject> _enemies = new();
        private int _score;

        private static bool CheckCollision(Vector2 first, Vector2 second)
        {
            return Math.Abs(first.X - second.X) < 5 && Math.Abs(first.Y - second.Y) < 5;
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
                        enemy.Destroy();
                        projectile.ShouldNotRender = true;
                        _score++;
                    }
                }
            }
            _projectiles.RemoveAll(p => p.ShouldNotRender);
            // CheckIfWin();
        }

        private void ShootPlayerProjectile()
        {
            GameObject arrow = new GameObject(GameObject.Transform.Position, 
                new Vector3(-MathHelper.PiOver2, 0, 0), 
                "_Resources/Models/arrow.obj");
            arrow.Model.Texture = Texture2DLoader.LoadFromFile("_Resources/Textures/sun.png");
            arrow.Model.Material = new Material(new Vector3(0.15f), new Vector3(1), new Vector3(0.2f));
            arrow.Transform.PositionModifier = f => arrow.Transform.Position + new Vector3(0, 0.2f, 0);
            arrow.Transform.ScaleModifier = f => new Vector3(1f, 1f, 1f);
            arrow.Model.VolumeShader = GameManager.Shaders["light"];
            arrow.Model.Bind();
            _projectiles.Add(arrow);
        }

        private bool _wasSpacePressed;

        public override void Update(float time)
        {
            CheckCollisionsWithEnemy();
            if (Keyboard.IsKeyDown(Key.Space) && !_wasSpacePressed)
            {
                ShootPlayerProjectile();
                _wasSpacePressed = true;
                return;
            }
            _wasSpacePressed = !Keyboard.IsKeyUp(Key.Space);
        }

        public override void Start()
        {
            _enemies = GameObject.FindWithComponent<EnemyComponent>().ToList();
        }
    }
}
