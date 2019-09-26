using System;
using System.Windows.Forms;
using System.Windows.Media;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using Color = System.Drawing.Color;
using System.Windows;
using InSpaceNoOneSeesYourShadow.Logic;

namespace InSpaceNoOneSeesYourShadow.GUI
{
    public partial class MainWindow : Window
    {
        private bool _canDraw;

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
            _ = Mouse.GetState(); //Necessary to prevent crashes later
            GLCanvas.MakeCurrent();
            CompositionTarget.Rendering += CompositionTarget_Rendering;
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
