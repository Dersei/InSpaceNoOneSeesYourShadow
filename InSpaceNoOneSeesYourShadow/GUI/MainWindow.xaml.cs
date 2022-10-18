using System;
using OpenTK.Graphics.OpenGL;
using Color = System.Drawing.Color;
using System.Windows;
using System.Windows.Input;
using InSpaceNoOneSeesYourShadow.Logic;
using OpenTK.Wpf;

namespace InSpaceNoOneSeesYourShadow.GUI
{
    public partial class MainWindow
    {
        private bool _canDraw;

        private readonly Game _game = new();

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = 0;
            Top = 0;
            InitializeComponent();
            var settings = new GLWpfControlSettings
            {
                MajorVersion = 4,
                MinorVersion = 6
            };
            OpenTkControl.Start(settings);
        }


        private PolygonMode _polygonMode;
        private int _counter = 1;
        private void ChangePolygonMode()
        {
            switch (_counter)
            {
                case 0:
                    _polygonMode = PolygonMode.Fill;
                    _counter++;
                    break;
                case 1:
                    _polygonMode = PolygonMode.Line;
                    _counter++;
                    break;
                case 2:
                    _polygonMode = PolygonMode.Point;
                    _counter = 0;
                    break;
            }
        }

        private void GLControl_Paint(TimeSpan delta)
        {
            if (!_canDraw) return;
            GL.PolygonMode(MaterialFace.FrontAndBack, _polygonMode);
            OnUpdateFrame();
            OnRenderFrame();
        }


        protected void OnRenderFrame()
        {
            GL.Viewport(0, 0, (int) OpenTkControl.Width, (int) OpenTkControl.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            _game.Draw();
        }

        protected void OnUpdateFrame()
        {
            _game.ProcessInput();
            _game.Update();
        }

        private void GLControl_Load(object sender, EventArgs e)
        {
            _game.InitProgram();
            GL.ClearColor(Color.White);
            _canDraw = true;
        }
        
        private void GLCanvas_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.P)
            {
                ChangePolygonMode();
            }
        }
    }
}
