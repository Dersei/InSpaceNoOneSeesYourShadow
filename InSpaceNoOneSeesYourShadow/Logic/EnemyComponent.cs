using System;
using System.Collections.Generic;
using System.Linq;
using InSpaceNoOneSeesYourShadow.Engine;
using InSpaceNoOneSeesYourShadow.Engine.ContentManagement;
using InSpaceNoOneSeesYourShadow.Engine.Core;
using InSpaceNoOneSeesYourShadow.Engine.Objects3D;
using OpenTK;

namespace InSpaceNoOneSeesYourShadow.Logic
{
    internal class EnemyComponent : Component
    {
        private readonly List<GameObject> _cachedEnemyProjectiles = new List<GameObject>();
        private readonly List<GameObject> _activeEnemyProjectiles = new List<GameObject>();
        private readonly int _maxProjectiles = 3;

        public override void Update(float time)
        {
            SpawnEnemyProjectiles();
            CheckCollisionsWithPlayer();
        }

        private static GameObject CreateEnemyProjectile()
        {
            GameObject arrow = new GameObject(Vector3.Zero,
                new Vector3(-MathHelper.PiOver2, -MathHelper.PiOver2, -MathHelper.PiOver2),
                "_Resources/Models/cone.obj");
            arrow.ShouldNotRender = true;
            arrow.Model.Texture = Texture2DLoader.LoadFromFile("_Resources/Textures/sun.png");
            arrow.Model.Material = new Material(new Vector3(0.15f), new Vector3(1), new Vector3(0.2f));
            arrow.Transform.PositionModifier = f => arrow.Transform.Position;
            arrow.Transform.ScaleModifier = f => new Vector3(1f, 1f, 1f);
            arrow.Model.VolumeShader = GameManager.Shaders["light"];
            arrow.Model.Bind();
            return arrow;
        }

        public void SpawnEnemyProjectiles()
        {
            var chance = Basic.Random.Next(0, 1000);
            if (chance > 5 || _activeEnemyProjectiles.Count > _maxProjectiles)
            {
                return;
            }
            var arrow = _cachedEnemyProjectiles.Except(_activeEnemyProjectiles).FirstOrDefault();
            if (arrow is null) return;
            arrow.ShouldNotRender = false;
            arrow.Transform.Position = GameObject.Transform.Position + new Vector3(0, 1, 0);
            arrow.Transform.PositionModifier = f => arrow.Transform.Position - new Vector3(0, 0.2f, 0);
            _activeEnemyProjectiles.Add(arrow);
        }

        public void CheckCollisionsWithPlayer()
        {
            if (_activeEnemyProjectiles.Count == 0) return;
            foreach (var projectile in _activeEnemyProjectiles.Where(p => !p.ShouldNotRender))
            {

                if (CheckCollision(projectile.Transform.Position.Xy, GameManager.PlayerPosition.Xy))
                {
                    projectile.ShouldNotRender = true;
                    //_health--;
                    //CheckIfDead();
                }
            }
            _activeEnemyProjectiles.RemoveAll(ep => ep.ShouldNotRender);
            _activeEnemyProjectiles.RemoveAll(ep => ep.Transform.Position.Y < -50);
            //CheckIfDead();
        }



        private static bool CheckCollision(Vector2 first, Vector2 second)
        {
            return Math.Abs(first.X - second.X) < 5 && Math.Abs(first.Y - second.Y) < 5;
        }

        public override void Start()
        {
            for (var i = 0; i < 3; i++)
            {
                _cachedEnemyProjectiles.Add(CreateEnemyProjectile());
            }
        }
    }
}
