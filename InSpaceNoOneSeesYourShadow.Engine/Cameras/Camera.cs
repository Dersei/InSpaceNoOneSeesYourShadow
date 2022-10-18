using OpenTK.Mathematics;

namespace InSpaceNoOneSeesYourShadow.Engine.Cameras
{
    public abstract class Camera
    {
        public Vector3 Position;
        public Vector3 Orientation;
        public float MoveSpeed;
        public float MouseSensitivity;
        public Matrix4 ViewMatrix => GetViewMatrix();
        public Matrix4 ProjectionMatrix { get; set; }
        public Matrix4 ViewProjectionMatrix => GetViewMatrix() * ProjectionMatrix;
        protected abstract Matrix4 GetViewMatrix();
        public abstract void Move(float x, float y, float z);
        public abstract void AddRotation(float x, float y);
    }
}