using System;
using System.Diagnostics;
using System.IO;
using OpenTK.Graphics.OpenGL;

namespace InSpaceNoOneSeesYourShadow.Helpers
{
    public class ShaderLoader
    {
        ///<summary>
        ///Create a program object and make current
        ///</summary>
        ///<param name="vShader">a vertex shader program</param>
        ///<param name="fShader">a fragment shader program</param>
        ///<param name="program">created program</param>
        ///<returns>
        ///return true, if the program object was created and successfully made current
        ///</returns>
        public static bool CreateAndStartProgram(string vShaderSource, string fShaderSource, out int program)
        {
            program = CreateProgram(vShaderSource, fShaderSource);
            if (program == 0)
            {
                Logger.Append("Failed to create program");
                return false;
            }

            GL.UseProgram(program);

            return true;
        }

        ///<summary>
        ///Load a shader from a file
        ///</summary>
        ///<param name="errorOutputFileName">a file name for error messages</param>
        ///<param name="fileName">a file name to a shader</param>
        ///<param name="shaderSource">a shader source string</param>
        public static void LoadShaderFromFile(string shaderFileName, out string shaderSource)
        {
            if (File.Exists(Logger.LogFileName))
            {
                // Clear File
                File.WriteAllText(Logger.LogFileName, "");
            }

            shaderSource = null;

            using (var sr = new StreamReader(shaderFileName))
            {
                shaderSource = sr.ReadToEnd();
            }
        }

        private static int CreateProgram(string vShader, string fShader)
        {
            // Create shader object
            var vertexShader = LoadShaderFromText(ShaderType.VertexShader, vShader);
            var fragmentShader = LoadShaderFromText(ShaderType.FragmentShader, fShader);
            if (vertexShader == 0 || fragmentShader == 0)
            {
                return 0;
            }

            // Create a program object
            var program = GL.CreateProgram();
            if (program == 0)
            {
                return 0;
            }

            // Attach the shader objects
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);

            // Link the program object
            GL.LinkProgram(program);

            // Check the result of linking
            GL.GetProgram(program, GetProgramParameterName.LinkStatus, out var status);
            if (status == 0)
            {
                var errorString = string.Format("Failed to link program: {0}" + Environment.NewLine, GL.GetProgramInfoLog(program));
                Logger.Append(errorString);
                GL.DeleteProgram(program);
                GL.DeleteShader(vertexShader);
                GL.DeleteShader(fragmentShader);
                return 0;
            }

            return program;
        }

        private static int LoadShaderFromText(ShaderType shaderType, string shaderSource)
        {
            // Create shader object
            var shader = GL.CreateShader(shaderType);
            if (shader == 0)
            {
                Logger.Append("Unable to create shader");
                return 0;
            }

            // Set the shader program
            GL.ShaderSource(shader, shaderSource);

            // Compile the shader
            GL.CompileShader(shader);

            // Check the result of compilation
            GL.GetShader(shader, ShaderParameter.CompileStatus, out var status);
            if (status == 0)
            {
                var errorString = $"Failed to compile {shaderType.ToString()} shader: {GL.GetShaderInfoLog(shader)}";
                Logger.Append(errorString);
                GL.DeleteShader(shader);
                return 0;
            }

            return shader;
        }
    }

    public class Logger
    {
        public static string LogFileName = "info.txt";

        /// <summary>
        /// Write a message to a log file
        /// </summary>
        /// <param name="message">a message that will append to a log file</param>
        public static void Append(string message)
        {
            File.AppendAllText(LogFileName, message + Environment.NewLine);
            Debug.WriteLine(message);
        }
    }
}
