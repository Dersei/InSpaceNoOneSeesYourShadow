using System;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using Utils;

namespace InSpaceNoOneSeesYourShadow
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _canDraw;
        private int _program;
        private int _nVertices;
        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void GLControl_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            GL.Viewport(0, 0, GLCanvas.Width, GLCanvas.Height);

            // Clear the render canvas with the current color
            GL.Clear(ClearBufferMask.ColorBufferBit);

            if (_canDraw)
            {
                GL.Uniform1(GL.GetUniformLocation(_program, "time"), 1);
                // Draw a triangle
                GL.DrawArrays(PrimitiveType.Triangles, 0, _nVertices);
            }

            GL.Flush();
            GLCanvas.SwapBuffers();
        }

        private void GLControl_Load(object sender, EventArgs e)
        {
            // Load shaders from files
            ShaderLoader.LoadShader("./Shaders/VertexShader.glsl", out var vShaderSource);
            ShaderLoader.LoadShader("./Shaders/FragmentShader.glsl", out var fShaderSource);
            if (vShaderSource == null || fShaderSource == null)
            {
                Logger.Append("Failed load shaders from files");
                return;
            }

            // Initialize the shaders
            if (!ShaderLoader.InitShaders(vShaderSource, fShaderSource, out _program))
            {
                Logger.Append("Failed to initialize the shaders");
                return;
            }

            // Write the positions of vertices to a vertex shader
            _nVertices = InitVertexBuffers();
            if (_nVertices <= 0)
            {
                Logger.Append("Failed to write the positions of vertices to a vertex shader");
                return;
            }

            // Specify the color for clearing
            GL.ClearColor(Color4.DarkSlateBlue);

            _canDraw = true;
        }

        private int InitVertexBuffers()
        {
            float[] vertices = new float[] { 0f, 0.5f, -0.5f, -0.5f, 0.5f, -0.5f };

            // Create a buffer object
            GL.GenBuffers(1, out int vertexBuffer);

            // Bind the buffer object to target
            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);

            // Write data into the buffer object
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);

            // Get the storage location of a_Position
            int aPosition = GL.GetAttribLocation(_program, "a_Position");
            if (aPosition < 0)
            {
                Logger.Append("Failed to get the storage location of a_Position");
                return -1;
            }
            // Assign the buffer object to a_Position variable
            GL.VertexAttribPointer(aPosition, 2, VertexAttribPointerType.Float, false, 0, 0);

            // Enable the assignment to a_Position variable
            GL.EnableVertexAttribArray(aPosition);

            return vertices.Length / 2;
        }

        private void WindowsFormsHost_Initialized(object sender, EventArgs e)
        {
            GLCanvas.MakeCurrent();
            _timer = new DispatcherTimer { Interval = new TimeSpan(100) };
            _timer.Tick += (s, args) => GLCanvas.Refresh();
            _timer.Start();
        }
    }
}
