using System;
using System.Collections.Generic;
using InSpaceNoOneSeesYourShadow.Engine;
using InSpaceNoOneSeesYourShadow.Engine.Cameras;
using InSpaceNoOneSeesYourShadow.Engine.Environment;
using InSpaceNoOneSeesYourShadow.Engine.Shaders;
using InSpaceNoOneSeesYourShadow.Engine.Utils;
using OpenTK.Mathematics;

namespace InSpaceNoOneSeesYourShadow.Logic
{
    internal class Game
    {
        public GameCamera Camera { get; } = new();
        private readonly Light _directionalLight = new(-Vector3.UnitZ, new Vector3(1f, 0.5f, 0.5f));
        private readonly Light _spotLight1 = new(new Vector3(1, 5, 1), new Vector3(1f, 1f, 1f));
        private readonly Light _spotLight2 = new(new Vector3(5, 10, 0), new Vector3(0.8f, 0.8f, 0f));
        private readonly Light _pointLight = new(new Vector3(1, 5, 1), new Vector3(1f, 0f, 0f));
        private readonly Dictionary<string, int> _textures = new();
        private readonly Dictionary<string, ShaderProgram> _shaders = new();
        private string _activeShader = "light";

        private DateTime _oldTime;
        public void Update()
        {
            GameManager.Time = _time += (DateTime.Now - _oldTime).Milliseconds / 1000f;
            GameManager.CurrentScene.Update(_time);
            _oldTime = DateTime.Now;
        }

        public void Draw() => GameManager.CurrentScene.Draw();

        public void InitProgram()
        {
            _shaders.Add("light", new LitShader());
            _shaders.Add("PBR", new PbrShader());
            GameManager.Shaders = _shaders;
            _activeShader = "PBR";
            _textures.Add("sun.png", ImageLoader.LoadImage("_Resources/Textures/sun.png"));
            _textures.Add("ship2_diffuse.bmp", ImageLoader.LoadImage("_Resources/Textures/ship2_diffuse.bmp"));
            _textures.Add("galaxy.png", ImageLoader.LoadImage("_Resources/Textures/galaxy.png"));
            _textures.Add("dread_ship_t.png", ImageLoader.LoadImage("_Resources/Textures/dread_ship_t.png"));
            GameManager.Textures = _textures;
            GameManager.Camera = Camera;
            GameManager.DirectionalLight = _directionalLight;
            GameManager.Spotlight = _spotLight2;
            GameManager.PointLight = _pointLight;
            GameManager.ActiveShader = _activeShader;
            Camera.Position = new Vector3(0f, 0f, -50f);
            Camera.ProjectionMatrix = Matrix4.CreateOrthographic(100, 100, 1.0f, 1000.0f);
            GameManager.CurrentScene = new Level1Scene();
            GameManager.CurrentScene.CreateScene();
        }

        private float _time;
        
        public void ProcessInput()
        {
        }
    }
}
