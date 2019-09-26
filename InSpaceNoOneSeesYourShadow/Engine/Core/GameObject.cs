using System.Collections.Generic;
using System.Linq;
using InSpaceNoOneSeesYourShadow.Engine.ContentManagement;
using InSpaceNoOneSeesYourShadow.Engine.Objects3D.Shapes;
using OpenTK;

namespace InSpaceNoOneSeesYourShadow.Engine.Core
{
    public class GameObject
    {
        public Transform Transform;
        public Model Model;
        public bool ShouldNotRender;
        public bool Enabled;
        private bool _isInGame;

        public List<Component> Components { get; } = new List<Component>();

        public void AddComponent(Component component)
        {
            Components.Add(component);
            component.GameObject = this;
        }

        public void AddComponent<T>() where T : Component, new()
        {
            var component = new T();
            Components.Add(component);
            component.GameObject = this;
        }

        public static IEnumerable<GameObject> FindWithComponent<T>() where T : Component
        {
            foreach (var gameObject in GameManager.CurrentScene.GameObjects)
            {
                if (gameObject.Components.Any(c => c is T))
                {
                    yield return gameObject;
                }
            }
        }

        public GameObject(Vector3 position, Vector3 rotation, Vector3 scale, Model model)
        {
            Transform = new Transform(position, rotation, scale);
            Model = model;
            GameManager.CurrentScene.AddGameObject(this);
            if (_isInGame) Start();
        }

        public GameObject(Vector3 position, Vector3 rotation, Vector3 scale, string modelName) : this(position, rotation, scale, ModelLoader.LoadFromFile(modelName)) { }
        public GameObject(Vector3 position, Model model) : this(position, Vector3.Zero, Vector3.One, model) { }
        public GameObject(Vector3 position, Vector3 rotation, Model model) : this(position, rotation, Vector3.One, model) { }
        public GameObject(Vector3 position, Vector3 rotation, string modelName) : this(position, rotation, Vector3.One, modelName) { }
        public GameObject(Vector3 position, string modelName) : this(position, Vector3.Zero, Vector3.One, modelName) { }

        public void Update(float value)
        {
            if (ShouldNotRender) return;
            Transform.Update(value);
            foreach (var component in Components)
            {
                component.Update(value);
            }
        }

        public void Start()
        {
            foreach (var component in Components)
            {
                component.Start();
            }
            _isInGame = true;
        }

        public void Draw()
        {
            if (ShouldNotRender) return;
            Model.BeginDraw();
            Model.VolumeShader.SetUniform("modelview", Transform.WorldMatrix * GameManager.Camera.ViewProjectionMatrix);
            Model.VolumeShader.SetUniform("model", Transform.WorldMatrix);
            Model.EndDraw();
        }

        public void Destroy()
        {
            Enabled = false;
            ShouldNotRender = true;
            GameManager.CurrentScene.RemoveGameObject(this);
        }
    }
}
