using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using InSpaceNoOneSeesYourShadow.Helpers;
using InSpaceNoOneSeesYourShadow.Helpers.Cameras;
using InSpaceNoOneSeesYourShadow.Objects3D;
using InSpaceNoOneSeesYourShadow.Objects3D.Shapes;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace InSpaceNoOneSeesYourShadow.GUI
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Vector2 _lastMousePos;

        private bool _canDraw;


        private Matrix4 _view = Matrix4.Identity;

        private readonly GameCamera _camera = new GameCamera();
        private readonly Light _directionalLight = new Light(-Vector3.UnitZ, new Vector3(1f, 0.5f, 0.5f));
        private readonly Light _spotLight1 = new Light(new Vector3(1, 5, 1), new Vector3(1f, 1f, 1f));
        private readonly Light _spotLight2 = new Light(new Vector3(5, 10, 0), new Vector3(0.8f, 0.8f, 0f));
        private readonly Light _pointLight = new Light(new Vector3(1, 5, 1), new Vector3(1f, 0f, 0f));
        //private Vector3[] _translations = new Vector3[20];
        private Random _random = new Random();
        private readonly List<ObjVolume> _objects = new List<ObjVolume>();
        private readonly List<ObjVolume> _enemies = new List<ObjVolume>();
        private readonly List<ObjVolume> _projectiles = new List<ObjVolume>();
        private readonly List<ObjVolume> _enemyProjectiles = new List<ObjVolume>();
        private readonly Dictionary<string, int> _textures = new Dictionary<string, int>();
        private readonly Dictionary<string, ShaderProgram> _shaders = new Dictionary<string, ShaderProgram>();
        private readonly Dictionary<string, Material> _materials = new Dictionary<string, Material>();
        private DispatcherTimer _timer;

        private string _activeShader = "light";

        private void LoadMaterials(string filename)
        {
            foreach (var mat in Material.LoadFromFile(filename))
                if (!_materials.ContainsKey(mat.Key)) _materials.Add(mat.Key, mat.Value);

            // Load textures
            foreach (var mat in _materials.Values)
            {
                if (File.Exists(mat.AmbientMap) && !_textures.ContainsKey(mat.AmbientMap)) _textures.Add(mat.AmbientMap, ImageLoader.LoadImage(mat.AmbientMap));

                if (File.Exists(mat.DiffuseMap) && !_textures.ContainsKey(mat.DiffuseMap)) _textures.Add(mat.DiffuseMap, ImageLoader.LoadImage(mat.DiffuseMap));

                if (File.Exists(mat.SpecularMap) && !_textures.ContainsKey(mat.SpecularMap)) _textures.Add(mat.SpecularMap, ImageLoader.LoadImage(mat.SpecularMap));

                if (File.Exists(mat.NormalMap) && !_textures.ContainsKey(mat.NormalMap)) _textures.Add(mat.NormalMap, ImageLoader.LoadImage(mat.NormalMap));

                if (File.Exists(mat.OpacityMap) && !_textures.ContainsKey(mat.OpacityMap)) _textures.Add(mat.OpacityMap, ImageLoader.LoadImage(mat.OpacityMap));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void InitProgram()
        {
            _lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);

            //GL.GenBuffers(1, out _iboElements);
            // Load shaders from file
            _shaders.Add("light", new ShaderProgram("_Resources/Shaders/vs_lit.glsl", "_Resources/Shaders/fs_lit.glsl", true));
            _shaders.Add("PBR", new ShaderProgram("_Resources/Shaders/vs_lit.glsl", "_Resources/Shaders/PBR.glsl", true));
            _activeShader = "PBR";
            //LoadMaterials("Models/tv.mtl");
            //LoadMaterials("Models/ship2.mtl");
            _textures.Add("sun.png", ImageLoader.LoadImage("_Resources/Textures/sun.png"));
            _textures.Add("ship2_diffuse.bmp", ImageLoader.LoadImage("_Resources/Textures/ship2_diffuse.bmp"));
            _textures.Add("galaxy.png", ImageLoader.LoadImage("_Resources/Textures/galaxy.png"));
            _textures.Add("dread_ship_t.png", ImageLoader.LoadImage("_Resources/Textures/dread_ship_t.png"));
            CreateScene();
            //CreateSkybox();

            // Move camera away from origin
            _camera.Position = new Vector3(0f, 0f, -50f);
            //_camera.Orientation = new Vector3(0, (float)Math.PI/4f, 0);
            //_camera.Orientation = new Vector3(1,0,0);
            //_camera.AddRotation(90, -90);
            //GL.ClipControl(ClipOrigin.LowerLeft, ClipDepthMode.ZeroToOne);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
        }

        private ObjVolume _playerShip;
        private float _time;

        private void CreateScene()
        {

            ObjVolume cubePlane = ObjVolume.LoadFromFile("_Resources/Models/simple_cube.obj");
            cubePlane.TextureId = _textures["galaxy.png"];
            cubePlane.Position += new Vector3(0f, 0f, 0f);
            cubePlane.Rotation = new Vector3(MathHelper.PiOver2, 0, 0f);
            cubePlane.Material = new Material(new Vector3(0.15f), new Vector3(1), new Vector3(0.2f));
            //cubePlane.PositionModifier = f => new Vector3(2 * (float)Math.Sin(MathHelper.DegreesToRadians(f * 80 % 360)), 0f, 2 * (float)Math.Cos(MathHelper.DegreesToRadians(f * 80 % 360)));
            cubePlane.ScaleModifier = f => new Vector3(50f, 0.1f, 50f);
            cubePlane.PbrValues = new ObjVolume.PBRValues
            {
                AO = 0.5f,
                Metallic = 0.5f,
                ReflectionStrength = 0.5f,
                Refraction = 4f,
                Roughness = 0.5f
            };
            cubePlane.VolumeShader = _shaders[_activeShader];
            cubePlane.Bind();
            _objects.Add(cubePlane);

            //ObjVolume ship = ObjVolume.LoadFromFile("Models/earth.obj");
            //ship.TextureId = _textures["ship2.png"];
            //ship.Position += new Vector3(20f, 0f, 0f);
            //ship.Rotation = new Vector3(MathHelper.Pi, 0, -0);
            //ship.Material = new Material(new Vector3(0.5f), new Vector3(1), new Vector3(0.8f));
            ////cubePlane.PositionModifier = f => new Vector3(2 * (float)Math.Sin(MathHelper.DegreesToRadians(f * 80 % 360)), 0f, 2 * (float)Math.Cos(MathHelper.DegreesToRadians(f * 80 % 360)));
            //ship.ScaleModifier = f => new Vector3(5f, 5f, 5f);
            //ship.VolumeShader = _shaders["light"];
            //ship.Bind();
            //_objects.Add(ship);

            for (int j = 0; j < 3; j++)
            {
                for (int i = 0; i < 6; i++)
                {
                    ObjVolume ship = ObjVolume.LoadFromFile("_Resources/Models/ship_dread_t.obj");
                    ship.TextureId = _textures["dread_ship_t.png"];
                    ship.Position += new Vector3(40f - 15 * i, 20f - 10 * j, 0f);
                    ship.PositionModifier = f => ship.Position + new Vector3((float)Math.Sin(_time) / 100f, -_time / 1000f, 0);
                    ship.Rotation = new Vector3(MathHelper.Pi, 0, -MathHelper.PiOver2);
                    ship.Material = new Material(new Vector3(0.5f), new Vector3(1), new Vector3(0.8f));
                    //cubePlane.PositionModifier = f => new Vector3(2 * (float)Math.Sin(MathHelper.DegreesToRadians(f * 80 % 360)), 0f, 2 * (float)Math.Cos(MathHelper.DegreesToRadians(f * 80 % 360)));
                    ship.ScaleModifier = f => new Vector3(0.005f, 0.005f, 0.005f);
                    ship.PbrValues = new ObjVolume.PBRValues
                    {
                        AO = 0.5f,
                        Metallic = 0.5f,
                        ReflectionStrength = (float)_random.NextDouble(),
                        Refraction = 0f,
                        Roughness = 0.5f
                    };
                    ship.VolumeShader = _shaders["PBR"];
                    ship.Bind();
                    _objects.Add(ship);
                    _enemies.Add(ship);
                }
            }


            ObjVolume playerShip = ObjVolume.LoadFromFile("_Resources/Models/ship2.obj");
            playerShip.Position += new Vector3(0f, -40f, -10f);
            playerShip.Rotation = new Vector3(-MathHelper.PiOver2, 0, 0);
            playerShip.Scale = new Vector3(0.05f, 0.05f, 0.05f);
            //cat.PositionModifier = f => new Vector3(10, 4, 0);
            //cat.Material = _materials["ship2mat"];
            playerShip.Material = new Material(new Vector3(0.5f), new Vector3(1), new Vector3(0.8f));
            playerShip.TextureId = _textures["ship2_diffuse.bmp"];
            playerShip.PbrValues = new ObjVolume.PBRValues
            {
                AO = 1f,
                Metallic = 0.5f,
                ReflectionStrength = 0f,
                Refraction = 0f,
                Roughness = 0f
            };
            playerShip.VolumeShader = _shaders["light"];
            playerShip.Bind();
            _playerShip = playerShip;
            _objects.Add(playerShip);
        }

        private void GLControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (!_canDraw) return;
            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);
            OnUpdateFrame();
            OnRenderFrame();
        }

        private void DrawVolume(ObjVolume volume)
        {
            volume.VolumeShader.EnableVertexAttribArrays();
            GL.UseProgram(volume.VolumeShader.ProgramId);
            GL.ActiveTexture(TextureUnit.Texture0);
            GL.BindTexture(TextureTarget.Texture2D, volume.TextureId);
            GL.UniformMatrix4(volume.VolumeShader.GetUniform("modelview"), false, ref volume.ModelViewProjectionMatrix);

            if (volume.VolumeShader.GetAttribute("vColor") != -1)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, volume.VolumeShader.GetBuffer("vColor"));
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(volume.GetColorData().Length * Vector3.SizeInBytes), volume.GetColorData(), BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(volume.VolumeShader.GetAttribute("vColor"), 3, VertexAttribPointerType.Float, true, 0, 0);
            }


            if (volume.VolumeShader.GetAttribute("maintexture") != -1)
            {
                GL.Uniform1(volume.VolumeShader.GetAttribute("maintexture"), volume.TextureId);
            }

            if (volume.VolumeShader.GetUniform("view") != -1)
            {
                GL.UniformMatrix4(volume.VolumeShader.GetUniform("view"), false, ref _view);
            }

            if (volume.VolumeShader.GetUniform("camPos") != -1)
            {
                GL.Uniform3(volume.VolumeShader.GetUniform("camPos"), _camera.Position);
            }

            if (volume.VolumeShader.GetUniform("model") != -1)
            {
                GL.UniformMatrix4(volume.VolumeShader.GetUniform("model"), false, ref volume.ModelMatrix);
            }

            if (volume.VolumeShader.GetUniform("material_ambient") != -1)
            {
                GL.Uniform3(volume.VolumeShader.GetUniform("material_ambient"), ref volume.Material.AmbientColor);
            }

            if (volume.VolumeShader.GetUniform("material_diffuse") != -1)
            {
                GL.Uniform3(volume.VolumeShader.GetUniform("material_diffuse"), ref volume.Material.DiffuseColor);
            }

            if (volume.VolumeShader.GetUniform("material_specular") != -1)
            {
                GL.Uniform3(volume.VolumeShader.GetUniform("material_specular"), ref volume.Material.SpecularColor);
            }

            if (volume.VolumeShader.GetUniform("material_specExponent") != -1)
            {
                GL.Uniform1(volume.VolumeShader.GetUniform("material_specExponent"), volume.Material.SpecularExponent);
            }

            if (volume.VolumeShader.GetUniform("light_position") != -1)
            {
                GL.Uniform3(volume.VolumeShader.GetUniform("light_position"), ref _directionalLight.Position);
            }

            if (volume.VolumeShader.GetUniform("light_color") != -1)
            {
                GL.Uniform3(volume.VolumeShader.GetUniform("light_color"), ref _directionalLight.Color);
            }

            if (volume.VolumeShader.GetUniform("light_diffuseIntensity") != -1)
            {
                GL.Uniform1(volume.VolumeShader.GetUniform("light_diffuseIntensity"), _directionalLight.DiffuseIntensity);
            }

            if (volume.VolumeShader.GetUniform("light_ambientIntensity") != -1)
            {
                GL.Uniform1(volume.VolumeShader.GetUniform("light_ambientIntensity"), _directionalLight.AmbientIntensity);
            }

            if (volume.VolumeShader.GetUniform("time") != -1)
            {
                GL.Uniform1(volume.VolumeShader.GetUniform("time"), _time);
            }

            if (volume.VolumeShader == _shaders["PBR"])
            {
                volume.VolumeShader.SetFloat("ao", volume.PbrValues.AO);
                volume.VolumeShader.SetFloat("metallic", volume.PbrValues.Metallic);
                volume.VolumeShader.SetFloat("roughness", volume.PbrValues.Roughness);
                volume.VolumeShader.SetFloat("reflectionStrength", volume.PbrValues.ReflectionStrength);
                volume.VolumeShader.SetFloat("refraction", volume.PbrValues.Refraction);
                volume.VolumeShader.SetVec3("dirLight.direction", _directionalLight.Position);
                volume.VolumeShader.SetVec3("dirLight.color", _directionalLight.Color);
                volume.VolumeShader.SetFloat("dirLight.lightStrength", 10f);
                volume.VolumeShader.SetVec3("pointLight.position", _pointLight.Position);
                volume.VolumeShader.SetVec3("pointLight.color", _pointLight.Color);
                volume.VolumeShader.SetFloat("spotLight[0].cutOff", (float)Math.Cos(MathHelper.RadiansToDegrees(10f)));
                volume.VolumeShader.SetFloat("spotLight[0].outerCutOff", (float)Math.Cos(MathHelper.RadiansToDegrees(80f)));
                //volume.VolumeShader.SetVec3("spotLight[0].color", _spotLight1.Color);
                volume.VolumeShader.SetVec3("spotLight[0].color", new Vector3(0, 1, 0));
                //volume.VolumeShader.SetVec3("spotLight[0].position", _spotLight1.Position);
                volume.VolumeShader.SetVec3("spotLight[0].position", _playerShip.Position);
                volume.VolumeShader.SetVec3("spotLight[0].direction", new Vector3(0, 1, 0));
                volume.VolumeShader.SetFloat("spotLight[1].cutOff", (float)Math.Cos(MathHelper.RadiansToDegrees(0f)));
                volume.VolumeShader.SetFloat("spotLight[1].outerCutOff", (float)Math.Cos(MathHelper.RadiansToDegrees(0f)));
                volume.VolumeShader.SetVec3("spotLight[1].color", _spotLight2.Color);
                volume.VolumeShader.SetVec3("spotLight[1].position", _spotLight2.Position);
                volume.VolumeShader.SetVec3("spotLight[1].direction", new Vector3(0, -1, 0));
            }

            GL.BindVertexArray(volume.VAO);
            GL.DrawElements(PrimitiveType.Triangles, volume.IndicesCount, DrawElementsType.UnsignedInt, 0);
            GL.BindVertexArray(0);

            //foreach (var child in volume.Children)
            //{
            //    GL.PolygonMode(MaterialFace.FrontAndBack, _polygonMode);
            //    DrawVolume(child, indexAt);
            //}
            volume.VolumeShader.DisableVertexAttribArrays();

        }

        protected void OnRenderFrame()
        {
            //CheckCollisionsWithEnemy();
            //CheckCollisionsWithPlayer();
            GL.Viewport(0, 0, GLCanvas.Width, GLCanvas.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            //GL.Enable(EnableCap.Blend);
            //GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.DstAlpha);
            //DrawSkybox();
            // _shaders[_activeShader].EnableVertexAttribArrays();
            // _directionalLight.Position += new Vector3((float)Math.Sin(_time));
            //SpawnEnemyProjectiles();
            foreach (var v in _objects)
            {
                if (v.ShouldNotRender)
                {
                    continue;
                }
                DrawVolume(v);
            }

            // _shaders[_activeShader].DisableVertexAttribArrays();

            GL.Flush();
            GLCanvas.SwapBuffers();
        }

        private DateTime _oldTime;

        protected void OnUpdateFrame()
        {

            ProcessInput();

            _time += (DateTime.Now - _oldTime).Milliseconds / 1000f;

            // Update model view matrices
            foreach (var v in _objects)
            {
                if (v.ShouldNotRender)
                {
                    continue;
                }
                v.Update(_time);
                v.CalculateModelMatrix();
                v.UpdateMatrices(_camera.GetViewMatrix() * Matrix4.CreateOrthographic(100, 100, 1.0f, 1000.0f));
            }

            _oldTime = DateTime.Now;
            _view = _camera.GetViewMatrix();
        }

        private void ProcessInput()
        {
            if (Keyboard.GetState().IsKeyDown(Key.Escape)) Close();

            //if (Keyboard.GetState().IsKeyDown(Key.W)) _camera.Move(0f, 0.1f, 0f);

            //if (Keyboard.GetState().IsKeyDown(Key.S)) _camera.Move(0f, -0.1f, 0f);

            //if (Keyboard.GetState().IsKeyDown(Key.A)) _camera.Move(-0.1f, 0f, 0f);

            //if (Keyboard.GetState().IsKeyDown(Key.D)) _camera.Move(0.1f, 0f, 0f);

            //if (Keyboard.GetState().IsKeyDown(Key.Q)) _camera.Move(0f, 0f, 0.1f);

            //if (Keyboard.GetState().IsKeyDown(Key.E)) _camera.Move(0f, 0f, -0.1f);

            //if (Keyboard.GetState().IsKeyDown(Key.W)) _camera.Move(0f, 0.1f, 0f);

            //if (Keyboard.GetState().IsKeyDown(Key.S)) _camera.Move(0f, -0.1f, 0f);

            if (Keyboard.GetState().IsKeyDown(Key.A)) _playerShip.Position += new Vector3(0.5f, 0, 0);

            if (Keyboard.GetState().IsKeyDown(Key.D)) _playerShip.Position += new Vector3(-0.5f, 0, 0);

            //if (Keyboard.GetState().IsKeyDown(Key.Q)) _camera.Move(0f, 0f, 0.1f);

            //if (Keyboard.GetState().IsKeyDown(Key.E)) _camera.Move(0f, 0f, -0.1f);


            if (Mouse.GetState().RightButton == ButtonState.Pressed)
            {
                var delta = _lastMousePos - new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                _lastMousePos += delta;

                _camera.AddRotation(delta.X, delta.Y);
                _lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            }

        }

        private void GLControl_Load(object sender, EventArgs e)
        {
            InitProgram();
            GL.ClearColor(Color.White);
            _canDraw = true;
        }


        private void WindowsFormsHost_Initialized(object sender, EventArgs e)
        {
            GLCanvas.MakeCurrent();
            _timer = new DispatcherTimer { Interval = new TimeSpan(1) };
            _timer.Tick += (s, args) => GLCanvas.Refresh();
            _timer.Start();
        }
    }
}
