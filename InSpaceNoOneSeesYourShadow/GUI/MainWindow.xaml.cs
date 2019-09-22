using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Threading;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Color = System.Drawing.Color;
using ButtonState = OpenTK.Input.ButtonState;
using OpenTK.Graphics;
using System.Windows;
using InSpaceNoOneSeesYourShadow.Logic;

namespace InSpaceNoOneSeesYourShadow.GUI
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _canDraw;

        private DispatcherTimer _timer;

        private readonly Game _game = new Game();

        public MainWindow()
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = 0;
            Top = 0;
            InitializeComponent();
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
            GLCanvas.Refresh();
        }

        private void GLControl_Paint(object sender, PaintEventArgs e)
        {
            if (!_canDraw) return;
            GL.PolygonMode(MaterialFace.FrontAndBack, _polygonMode);
            OnUpdateFrame();
            OnRenderFrame();
        }


        protected void OnRenderFrame()
        {
            GL.Viewport(0, 0, GLCanvas.Width, GLCanvas.Height);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            _game.Draw();
            GLCanvas.SwapBuffers();
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


        private void WindowsFormsHost_Initialized(object sender, EventArgs e)
        {
            MouseState state = Mouse.GetState();
            MouseButton mb = MouseButton.Button1;
            GLCanvas.MakeCurrent();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1) };
            _timer.Tick += (s, args) =>
            {

                //_timer.Start();
            };
            CompositionTarget.Rendering += CompositionTarget_Rendering;
            //_timer.Start();
        }

        private void CompositionTarget_Rendering(object sender, EventArgs e)
        {
            GLCanvas.Refresh();
        }

        private void GLCanvas_OnPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.P)
            {
                ChangePolygonMode();
            }
        }
    }
}
