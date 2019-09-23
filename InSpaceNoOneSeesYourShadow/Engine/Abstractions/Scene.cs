using System.Collections.Generic;
using InSpaceNoOneSeesYourShadow.Engine.Core;

namespace InSpaceNoOneSeesYourShadow.Engine.Abstractions
{
    public abstract class Scene
    {
        protected List<GameObject> _gameObjectsToAdd = new List<GameObject>();
        protected List<GameObject> _gameObjectsToRemove = new List<GameObject>();
        protected List<GameObject> _gameObjects = new List<GameObject>();
        public List<GameObject> GameObjects => _gameObjects;
        public abstract void CreateScene();

        public virtual void Update(float time) => ReorganizeCollections();
        public abstract void Draw();
        public void AddGameObject(GameObject gameObject) => _gameObjectsToAdd.Add(gameObject);
        public void RemoveGameObject(GameObject gameObject) => _gameObjectsToRemove.Add(gameObject);

        protected void ReorganizeCollections()
        {
            if (_gameObjectsToAdd.Count == 0 && _gameObjectsToRemove.Count == 0) return;
            foreach (var newGameObject in _gameObjectsToAdd)
            {
                _gameObjects.Add(newGameObject);
            }
            foreach (var gameObjectToRemove in _gameObjectsToRemove)
            {
                _gameObjects.Remove(gameObjectToRemove);
            }
            _gameObjectsToAdd.Clear();
            _gameObjectsToRemove.Clear();
        }
    }
}
