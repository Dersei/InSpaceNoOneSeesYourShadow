using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InSpaceNoOneSeesYourShadow.Engine.Objects3D.Shapes;
using InSpaceNoOneSeesYourShadow.Engine.Utils;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace InSpaceNoOneSeesYourShadow.Engine.Shaders
{
    public class ShaderProgram
    {
        public int ProgramId { get; }

        public int VShaderId
        {
            get => _vShaderId;
            set => _vShaderId = value;
        }

        public int FShaderId
        {
            get => _fShaderId;
            set => _fShaderId = value;
        }

        public int UniformCount
        {
            get => _uniformCount;
            set => _uniformCount = value;
        }

        public int AttributeCount
        {
            get => _attributeCount;
            set => _attributeCount = value;
        }

        private int _vShaderId = -1;
        private int _fShaderId = -1;
        private int _gShaderId = -1;
        private int _attributeCount;
        private int _uniformCount;

        public Dictionary<string, AttributeInfo> Attributes = new Dictionary<string, AttributeInfo>();
        public Dictionary<string, UniformInfo> Uniforms = new Dictionary<string, UniformInfo>();
        public Dictionary<string, uint> Buffers = new Dictionary<string, uint>();
        public Model Model;

        public ShaderProgram()
        {
            ProgramId = GL.CreateProgram();
        }

        private void LoadShader(string code, ShaderType type, out int address)
        {
            address = GL.CreateShader(type);
            GL.ShaderSource(address, code);
            GL.CompileShader(address);
            GL.AttachShader(ProgramId, address);
            Logger.LogConsole(GL.GetShaderInfoLog(address));
        }

        public void LoadShaderFromString(string code, ShaderType type)
        {
            if (type == ShaderType.VertexShader)
            {
                LoadShader(code, type, out _vShaderId);
            }
            else if (type == ShaderType.FragmentShader)
            {
                LoadShader(code, type, out _fShaderId);
            }
        }

        public void LoadShaderFromFile(string filename, ShaderType type)
        {
            using var streamReader = new StreamReader(filename);
            if (type == ShaderType.VertexShader)
            {
                LoadShader(streamReader.ReadToEnd(), type, out _vShaderId);
            }
            else if (type == ShaderType.FragmentShader)
            {
                LoadShader(streamReader.ReadToEnd(), type, out _fShaderId);
            }
            else if (type == ShaderType.GeometryShader)
            {
                LoadShader(streamReader.ReadToEnd(), type, out _gShaderId);
            }
        }

        public void Link()
        {
            GL.LinkProgram(ProgramId);

            Logger.LogConsole(GL.GetProgramInfoLog(ProgramId));

            GL.GetProgram(ProgramId, GetProgramParameterName.ActiveAttributes, out _attributeCount);
            GL.GetProgram(ProgramId, GetProgramParameterName.ActiveUniforms, out _uniformCount);

            for (var i = 0; i < AttributeCount; i++)
            {
                var info = new AttributeInfo();

                GL.GetActiveAttrib(ProgramId, i, 256, out _, out info.Size, out info.Type, out var name);

                info.Name = name;
                info.Address = GL.GetAttribLocation(ProgramId, info.Name);
                Attributes.Add(name, info);
            }

            for (var i = 0; i < UniformCount; i++)
            {
                var info = new UniformInfo();

                GL.GetActiveUniform(ProgramId, i, 256, out _, out info.Size, out info.Type, out var name);

                info.Name = name;
                Uniforms.Add(name, info);
                info.Address = GL.GetUniformLocation(ProgramId, info.Name);
            }
        }

        public void GenBuffers()
        {
            for (var i = 0; i < Attributes.Count; i++)
            {
                GL.GenBuffers(1, out uint buffer);

                Buffers.Add(Attributes.Values.ElementAt(i).Name, buffer);
            }

            for (var i = 0; i < Uniforms.Count; i++)
            {
                GL.GenBuffers(1, out uint buffer);

                Buffers.Add(Uniforms.Values.ElementAt(i).Name, buffer);
            }
        }

        public void EnableVertexAttributesArrays()
        {
            for (var i = 0; i < Attributes.Count; i++)
            {
                GL.EnableVertexAttribArray(Attributes.Values.ElementAt(i).Address);
            }
        }

        public void DisableVertexAttributesArrays()
        {
            for (var i = 0; i < Attributes.Count; i++)
            {
                GL.DisableVertexAttribArray(Attributes.Values.ElementAt(i).Address);
            }
        }

        public int GetAttribute(string name)
        {
            if (Attributes.ContainsKey(name))
            {
                return Attributes[name].Address;
            }

            return -1;
        }

        public int GetUniform(string name)
        {
            if (Uniforms.ContainsKey(name))
            {
                return Uniforms[name].Address;
            }

            return -1;
        }

        public uint GetBuffer(string name)
        {
            return Buffers.ContainsKey(name) ? Buffers[name] : 0;
        }


        public int LoadGeometryShader(string shader)
        {
            LoadShaderFromFile(shader, ShaderType.GeometryShader);
            return _gShaderId;
        }

        public ShaderProgram(string vShader, string fShader, string geometryShader, bool fromFile = false)
        {
            ProgramId = GL.CreateProgram();

            if (fromFile)
            {
                LoadShaderFromFile(vShader, ShaderType.VertexShader);
                LoadShaderFromFile(fShader, ShaderType.FragmentShader);
                LoadShaderFromFile(geometryShader, ShaderType.GeometryShader);
            }
            else
            {
                LoadShaderFromString(vShader, ShaderType.VertexShader);
                LoadShaderFromString(fShader, ShaderType.FragmentShader);
            }

            Link();
            GenBuffers();
        }

        public ShaderProgram(string vShader, string fShader, bool fromFile = false)
        {
            ProgramId = GL.CreateProgram();

            if (fromFile)
            {
                LoadShaderFromFile(vShader, ShaderType.VertexShader);
                LoadShaderFromFile(fShader, ShaderType.FragmentShader);
            }
            else
            {
                LoadShaderFromString(vShader, ShaderType.VertexShader);
                LoadShaderFromString(fShader, ShaderType.FragmentShader);
            }

            Link();
            GenBuffers();
        }

        public bool SetUniform(string name, int value)
        {
            if (Uniforms.ContainsKey(name))
            {
                GL.Uniform1(Uniforms[name].Address, value);
                return true;
            }

            return false;
        }

        public bool SetUniform(string name, float value)
        {
            if (Uniforms.ContainsKey(name))
            {
                GL.Uniform1(Uniforms[name].Address, value);
                return true;
            }

            return false;
        }

        public bool SetUniform(string name, double value)
        {
            if (Uniforms.ContainsKey(name))
            {
                GL.Uniform1(Uniforms[name].Address, value);
                return true;
            }

            return false;
        }

        public bool SetUniform(string name, Vector3 value)
        {
            if (Uniforms.ContainsKey(name))
            {
                GL.Uniform3(Uniforms[name].Address, value);
                return true;
            }

            return false;
        }

        public bool SetUniform(string name, Matrix4 value, bool transpose = false)
        {
            if (Uniforms.ContainsKey(name))
            {
                GL.UniformMatrix4(Uniforms[name].Address, transpose, ref value);
                return true;
            }

            return false;
        }

        public bool SetUniform(string name, bool value)
        {
            if (Uniforms.ContainsKey(name))
            {
                GL.Uniform1(Uniforms[name].Address, value ? 1 : 0);
                return true;
            }

            return false;
        }

        public bool SetAttribute(string name, Vector3[] value)
        {
            if (Attributes.ContainsKey(name))
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, GetBuffer(name));
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(value.Length * Vector3.SizeInBytes), value, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(Attributes[name].Address, 3, VertexAttribPointerType.Float, true, 0, 0);
                return true;
            }

            return false;
        }

        public virtual void Draw()
        {
            SetUniform("view", GameManager.Camera.ViewMatrix);
            SetUniform("camPos", GameManager.Camera.Position);
            SetUniform("material_ambient", Model.Material.AmbientColor);
            SetUniform("material_diffuse", Model.Material.DiffuseColor);
            SetUniform("material_specular", Model.Material.SpecularColor);
            SetUniform("material_specExponent", Model.Material.SpecularExponent);
            SetUniform("light_position", GameManager.DirectionalLight.Position);
            SetUniform("light_color", GameManager.DirectionalLight.Color);
            SetUniform("light_diffuseIntensity", GameManager.DirectionalLight.DiffuseIntensity);
            SetUniform("light_ambientIntensity", GameManager.DirectionalLight.AmbientIntensity);
            SetUniform("time", GameManager.Time);
        }

        public class UniformInfo
        {
            public string Name = "";
            public int Address = -1;
            public int Size;
            public ActiveUniformType Type;
        }

        public class AttributeInfo
        {
            public string Name = "";
            public int Address = -1;
            public int Size;
            public ActiveAttribType Type;
        }
    }
}
