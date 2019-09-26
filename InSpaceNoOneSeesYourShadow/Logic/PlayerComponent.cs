using InSpaceNoOneSeesYourShadow.Engine;
using InSpaceNoOneSeesYourShadow.Engine.Core;
using OpenTK;
using OpenTK.Input;

namespace InSpaceNoOneSeesYourShadow.Logic
{
    internal class PlayerComponent : Component
    {
        public override void Update(float time)
        {
            if (Keyboard.GetState().IsKeyDown(Key.A))
            {
                if (GameObject.Transform.Position.X > 45)
                    return;
                GameObject.Transform.Position += new Vector3(0.5f, 0, 0);
            }
            if (Keyboard.GetState().IsKeyDown(Key.D))
            {
                if (GameObject.Transform.Position.X < -45)
                    return;
                GameObject.Transform.Position += new Vector3(-0.5f, 0, 0);
            }

            GameManager.PlayerPosition = GameObject.Transform.Position;
        }
    }
}
