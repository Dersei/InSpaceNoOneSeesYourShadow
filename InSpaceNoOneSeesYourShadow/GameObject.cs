using InSpaceNoOneSeesYourShadow.Objects3D.Shapes;
using OpenTK;

namespace InSpaceNoOneSeesYourShadow
{
    internal class GameObject
    {
        public Transform Transform;
        public ObjVolume Model;

        public GameObject(Vector3 position, Vector3 rotation, Vector3 scale, ObjVolume model)
        {
            Transform = new Transform(position, rotation, scale);
            Model = model;
        }

        public void Update(float value)
        {
            Transform.Update(value);
        }

        public void Draw()
        {

        }

    }
}
