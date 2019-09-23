namespace InSpaceNoOneSeesYourShadow.Engine.Core
{
    public abstract class Component
    {
        public GameObject GameObject;
        private bool _enabled = true;
        public bool Enabled
        {
            get => _enabled && GameObject.Enabled;
            set => _enabled = value;
        }
        
        public abstract void Update(float time);
        public virtual void Start() { }
    }
}
