using System.Collections.Generic;
using InSpaceNoOneSeesYourShadow.Engine.Core;

namespace InSpaceNoOneSeesYourShadow.Engine.Abstractions
{
    public abstract class Scene
    {
        protected List<GameObject> GameObjectsToAdd = new List<GameObject>();
        protected List<GameObject> GameObjectsToRemove = new List<GameObject>();
        protected List<GameObject> _gameObjects = new List<GameObject>();
        public IReadOnlyList<GameObject> GameObjects => _gameObjects;
        public abstract void CreateScene();

        public virtual void Update(float time) => ReorganizeCollections();
        public abstract void Draw();
        public void AddGameObject(GameObject gameObject) => GameObjectsToAdd.Add(gameObject);
        public void RemoveGameObject(GameObject gameObject) => GameObjectsToRemove.Add(gameObject);

        protected void ReorganizeCollections()
        {
            if (GameObjectsToAdd.Count == 0 && GameObjectsToRemove.Count == 0) return;
            foreach (var newGameObject in GameObjectsToAdd)
            {
                _gameObjects.Add(newGameObject);
            }
            foreach (var gameObjectToRemove in GameObjectsToRemove)
            {
                _gameObjects.Remove(gameObjectToRemove);
            }
            GameObjectsToAdd.Clear();
            GameObjectsToRemove.Clear();
        }
    }
}
