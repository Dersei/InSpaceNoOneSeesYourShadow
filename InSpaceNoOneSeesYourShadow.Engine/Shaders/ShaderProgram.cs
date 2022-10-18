using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InSpaceNoOneSeesYourShadow.Engine.Objects3D;
using InSpaceNoOneSeesYourShadow.Engine.Utils;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace InSpaceNoOneSeesYourShadow.Engine.Shaders
{
    public abstract class ShaderProgram
    {
        public int ProgramId { get; }
        public int VertexShaderId => _vertexShaderId;
        public int FragmentShaderId => _fragmentShaderId;
        public int GeometryShaderId => _geometryShaderId;

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

        private int _vertexShaderId = -1;
        private int _fragmentShaderId = -1;
        private int _geometryShaderId = -1;
        private int _attributeCount;
        private int _uniformCount;

        private readonly Dictionary<string, AttributeInfo> _attributes = new();
        private readonly Dictionary<string, UniformInfo> _uniforms = new();
        private readonly Dictionary<string, uint> _buffers = new();
        public Model Model;

        private void LoadShader(string code, ShaderType type, out int address)
        {
            address = GL.CreateShader(type);
            GL.ShaderSource(address, code);
            GL.CompileShader(address);
            GL.AttachShader(ProgramId, address);
            Logger.LogConsole(GL.GetShaderInfoLog(address));
        }

        private void LoadShaderFromString(string code, ShaderType type)
        {
            if (type == ShaderType.VertexShader)
            {
                LoadShader(code, type, out _vertexShaderId);
            }
            else if (type == ShaderType.FragmentShader)
            {
                LoadShader(code, type, out _fragmentShaderId);
            }
            else if (type == ShaderType.GeometryShader)
            {
                LoadShader(code, type, out _geometryShaderId);
            }
        }

        private void LoadShaderFromFile(string filename, ShaderType type)
        {
            using var streamReader = new StreamReader(filename);
            if (type == ShaderType.VertexShader)
            {
                LoadShader(streamReader.ReadToEnd(), type, out _vertexShaderId);
            }
            else if (type == ShaderType.FragmentShader)
            {
                LoadShader(streamReader.ReadToEnd(), type, out _fragmentShaderId);
            }
            else if (type == ShaderType.GeometryShader)
            {
                LoadShader(streamReader.ReadToEnd(), type, out _geometryShaderId);
            }
        }

        private void Link()
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
                _attributes.Add(name, info);
            }

            for (var i = 0; i < UniformCount; i++)
            {
                var info = new UniformInfo();

                GL.GetActiveUniform(ProgramId, i, 256, out _, out info.Size, out info.Type, out var name);

                info.Name = name;
                _uniforms.Add(name, info);
                info.Address = GL.GetUniformLocation(ProgramId, info.Name);
            }
        }

        private void GenBuffers()
        {
            for (var i = 0; i < _attributes.Count; i++)
            {
                GL.GenBuffers(1, out uint buffer);

                _buffers.Add(_attributes.Values.ElementAt(i).Name, buffer);
            }

            for (var i = 0; i < _uniforms.Count; i++)
            {
                GL.GenBuffers(1, out uint buffer);

                _buffers.Add(_uniforms.Values.ElementAt(i).Name, buffer);
            }
        }

        public void EnableVertexAttributesArrays()
        {
            for (var i = 0; i < _attributes.Count; i++)
            {
                GL.EnableVertexAttribArray(_attributes.Values.ElementAt(i).Address);
            }
        }

        public void DisableVertexAttributesArrays()
        {
            for (var i = 0; i < _attributes.Count; i++)
            {
                GL.DisableVertexAttribArray(_attributes.Values.ElementAt(i).Address);
            }
        }

        public int GetAttribute(string name)
        {
            if (_attributes.ContainsKey(name))
            {
                return _attributes[name].Address;
            }

            return -1;
        }

        public int GetUniform(string name)
        {
            if (_uniforms.ContainsKey(name))
            {
                return _uniforms[name].Address;
            }

            return -1;
        }

        public uint GetBuffer(string name)
        {
            return _buffers.ContainsKey(name) ? _buffers[name] : 0;
        }

        protected ShaderProgram(string vertexShader, string fragmentShader, string geometryShader, bool fromFile = true)
        {
            ProgramId = GL.CreateProgram();

            if (fromFile)
            {
                LoadShaderFromFile(vertexShader, ShaderType.VertexShader);
                LoadShaderFromFile(fragmentShader, ShaderType.FragmentShader);
                LoadShaderFromFile(geometryShader, ShaderType.GeometryShader);
            }
            else
            {
                LoadShaderFromString(vertexShader, ShaderType.VertexShader);
                LoadShaderFromString(fragmentShader, ShaderType.FragmentShader);
                LoadShaderFromString(geometryShader, ShaderType.GeometryShader);
            }

            Link();
            GenBuffers();
        }

        protected ShaderProgram(string vertexShader, string fragmentShader, bool fromFile = true)
        {
            ProgramId = GL.CreateProgram();

            if (fromFile)
            {
                LoadShaderFromFile(vertexShader, ShaderType.VertexShader);
                LoadShaderFromFile(fragmentShader, ShaderType.FragmentShader);
            }
            else
            {
                LoadShaderFromString(vertexShader, ShaderType.VertexShader);
                LoadShaderFromString(fragmentShader, ShaderType.FragmentShader);
            }

            Link();
            GenBuffers();
        }

        public bool SetUniform(string name, int value)
        {
            if (_uniforms.ContainsKey(name))
            {
                GL.Uniform1(_uniforms[name].Address, value);
                return true;
            }

            return false;
        }

        public bool SetUniform(string name, float value)
        {
            if (_uniforms.ContainsKey(name))
            {
                GL.Uniform1(_uniforms[name].Address, value);
                return true;
            }

            return false;
        }

        public bool SetUniform(string name, double value)
        {
            if (_uniforms.ContainsKey(name))
            {
                GL.Uniform1(_uniforms[name].Address, value);
                return true;
            }

            return false;
        }

        public bool SetUniform(string name, Vector3 value)
        {
            if (_uniforms.ContainsKey(name))
            {
                GL.Uniform3(_uniforms[name].Address, value);
                return true;
            }

            return false;
        }

        public bool SetUniform(string name, Matrix4 value, bool transpose = false)
        {
            if (_uniforms.ContainsKey(name))
            {
                GL.UniformMatrix4(_uniforms[name].Address, transpose, ref value);
                return true;
            }

            return false;
        }

        public bool SetUniform(string name, bool value)
        {
            if (_uniforms.ContainsKey(name))
            {
                GL.Uniform1(_uniforms[name].Address, value ? 1 : 0);
                return true;
            }

            return false;
        }

        public bool SetAttribute(string name, Vector3[] value)
        {
            if (_attributes.ContainsKey(name))
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, GetBuffer(name));
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(value.Length * Vector3.SizeInBytes), value, BufferUsageHint.StaticDraw);
                GL.VertexAttribPointer(_attributes[name].Address, 3, VertexAttribPointerType.Float, true, 0, 0);
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

        private class UniformInfo
        {
            public string Name = "";
            public int Address = -1;
            public int Size;
            public ActiveUniformType Type;
        }

        private class AttributeInfo
        {
            public string Name = "";
            public int Address = -1;
            public int Size;
            public ActiveAttribType Type;
        }
    }
}
