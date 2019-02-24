using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using InSpaceNoOneSeesYourShadow.Helpers;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace InSpaceNoOneSeesYourShadow.Objects3D.Shapes
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct Vertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TexCoords;
    }

    internal class ObjVolume : Volume
    {
        public ObjVolume() : base()
        {

        }

        public PBRValues PbrValues { get; set; }

        public struct PBRValues
        {
            public float AO { get; set; }
            public float Metallic { get; set; }
            public float Roughness { get; set; }
            public float ReflectionStrength { get; set; }
            public float Refraction { get; set; }
        }

        private readonly List<Tuple<FaceVertex, FaceVertex, FaceVertex>> _faces = new List<Tuple<FaceVertex, FaceVertex, FaceVertex>>();
        public override int VerticesCount => _faces.Count * 3;

        public override int IndicesCount => _faces.Count * 3;

        public override int ColorDataCount => _faces.Count * 3;

        public override int TextureCoordsCount => _faces.Count * 3;

        public List<Vertex> VerticesStruct = new List<Vertex>();

        public bool ShouldNotRender { get; set; }

        public int VAO;
        public int VBO;
        public int EBO;

        public ShaderProgram VolumeShader { get; set; }

        public unsafe void Bind(bool onlyStructs = false)
        {
            var normals = GetNormals();
            var texcoords = GetTextureCoords();
            var vertices = GetVertices();

            for (int i = 0; i < vertices.Length; i++)
            {
                VerticesStruct.Add(new Vertex()
                {
                    Normal = normals[i],
                    TexCoords = texcoords[i],
                    Position = vertices[i]
                });
            }

            if (onlyStructs) return;


            VAO = GL.GenVertexArray();
            VBO = GL.GenBuffer();
            EBO = GL.GenBuffer();

            GL.BindVertexArray(VAO);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);

            GL.BufferData(BufferTarget.ArrayBuffer, VerticesStruct.Count * sizeof(Vertex), VerticesStruct.ToArray(), BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            GL.BufferData(BufferTarget.ElementArrayBuffer, GetIndices().Length * sizeof(int), GetIndices(), BufferUsageHint.StaticDraw);

            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(VolumeShader.GetAttribute("vPosition"), 3, VertexAttribPointerType.Float, false, sizeof(Vertex), 0);
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(VolumeShader.GetAttribute("vNormal"), 3, VertexAttribPointerType.Float, true, sizeof(Vertex), Vector3.SizeInBytes);
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(VolumeShader.GetAttribute("texcoord"), 2, VertexAttribPointerType.Float, true, sizeof(Vertex), Vector3.SizeInBytes * 2);

            GL.BindVertexArray(0);

        }

        public void CreateDictionary()
        {
            
        }

        public void Draw()
        {

        }
        /// <summary>
        /// Get vertices for this object
        /// </summary>
        /// <returns></returns>
        public override Vector3[] GetVertices()
        {
            List<Vector3> vertices = new List<Vector3>();

            foreach (var face in _faces)
            {
                vertices.Add(face.Item1.Position);
                vertices.Add(face.Item2.Position);
                vertices.Add(face.Item3.Position);
            }

            foreach (var child in Children)
            {
                vertices.AddRange(child.GetVertices());
            }

            return vertices.ToArray();
        }

        /// <summary>
        /// Get indices to draw this object
        /// </summary>
        /// <param name="offset">value to number first vertex in object</param>
        /// <returns>Array of indices offset to match buffered data</returns>
        public override int[] GetIndices(int offset = 0)
        {
            var indices = Enumerable.Range(offset, IndicesCount).ToList();
            var off = VerticesCount + offset;

            foreach (var child in Children)
            {
                indices.AddRange(child.GetIndices(off));
                off += child.VerticesCount;
            }

            return indices.ToArray();
        }

        /// <summary>
        /// Get color data.
        /// </summary>
        /// <returns></returns>
        public override Vector3[] GetColorData()
        {
            var colors = new Vector3[ColorDataCount].ToList();
            foreach (var child in Children)
            {
                colors.AddRange(child.GetColorData());
            }
            return colors.ToArray();
        }

        /// <summary>
        /// Get texture coordinates
        /// </summary>
        /// <returns></returns>
        public override Vector2[] GetTextureCoords()
        {
            List<Vector2> coords = new List<Vector2>();

            foreach (var face in _faces)
            {
                coords.Add(face.Item1.TextureCoords);
                coords.Add(face.Item2.TextureCoords);
                coords.Add(face.Item3.TextureCoords);
            }

            foreach (var child in Children)
            {
                coords.AddRange(child.GetTextureCoords());
            }
            return coords.ToArray();
        }


        /// <summary>
        /// Calculates the model matrix from transforms
        /// </summary>
        public override void CalculateModelMatrix()
        {
            ModelMatrix = Matrix4.CreateScale(Scale) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y) * Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateTranslation(Position);
            if (Parent != null)
            {
                ModelMatrix *= Parent.ModelMatrix;
            }
            foreach (var volume in Children)
            {
                volume.CalculateModelMatrix();
            }
        }

        public override void UpdateMatrices(Matrix4 newValue)
        {
            ViewProjectionMatrix = newValue;
            ModelViewProjectionMatrix = ModelMatrix * ViewProjectionMatrix;

            foreach (var child in Children)
            {
                child.UpdateMatrices(newValue);
            }
        }

        /// <summary>
        /// Loads a model from a file.
        /// </summary>
        /// <param name="filename">File to load model from</param>
        /// <returns>ObjVolume of loaded model</returns>
        public static ObjVolume LoadFromFile(string filename)
        {
            ObjVolume obj = new ObjVolume();
            try
            {
                using (StreamReader reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
                {
                    obj = LoadFromString(reader.ReadToEnd());
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($@"File not found: {filename}");
            }
            catch (Exception)
            {
                Console.WriteLine($@"Error loading file: {filename}");
            }

            return obj;
        }

        public static ObjVolume LoadFromString(string obj)
        {
            // Seperate lines from the file
            var lines = new List<string>(obj.Split('\n'));

            // Lists to hold model data
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var texs = new List<Vector2>();
            var faces = new List<Tuple<TempVertex, TempVertex, TempVertex>>();

            // Base values
            vertices.Add(new Vector3());
            texs.Add(new Vector2());
            normals.Add(new Vector3());

            var currentIndex = 0;

            // Read file line by line
            foreach (var line in lines)
            {
                if (line.StartsWith("v ")) // Vertex definition
                {
                    // Cut off beginning of line
                    var temp = line.Substring(2);

                    var vec = new Vector3();

                    if (temp.Trim().Count(c => c == ' ') == 2) // Check if there's enough elements for a vertex
                    {
                        var vertparts = temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Attempt to parse each part of the vertice
                        var style = System.Globalization.NumberStyles.Number;
                        var culture = System.Globalization.CultureInfo.CreateSpecificCulture("en-GB");
                        var success = float.TryParse(vertparts[0], style, culture, out vec.X);
                        success |= float.TryParse(vertparts[1], style, culture, out vec.Y);
                        success |= float.TryParse(vertparts[2], style, culture, out vec.Z);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing vertex: {0}", line);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error parsing vertex: {0}", line);
                    }

                    vertices.Add(vec);
                }
                else if (line.StartsWith("vt ")) // Texture coordinate
                {
                    // Cut off beginning of line
                    var temp = line.Substring(2);

                    var vec = new Vector2();

                    if (temp.Trim().Count(c => c == ' ') > 0) // Check if there's enough elements for a vertex
                    {
                        var texcoordparts = temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        // Attempt to parse each part of the vertice
                        var style = System.Globalization.NumberStyles.Number;
                        var culture = System.Globalization.CultureInfo.CreateSpecificCulture("en-GB");
                        var success = float.TryParse(texcoordparts[0], style, culture, out vec.X);
                        success |= float.TryParse(texcoordparts[1], style, culture, out vec.Y);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing texture coordinate: {0}", line);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error parsing texture coordinate: {0}", line);
                    }

                    texs.Add(vec);
                }
                else if (line.StartsWith("vn ")) // Normal vector
                {
                    // Cut off beginning of line
                    var temp = line.Substring(2);

                    var vec = new Vector3();

                    if (temp.Trim().Count(c => c == ' ') == 2) // Check if there's enough elements for a normal
                    {
                        var vertparts = temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        var style = System.Globalization.NumberStyles.Number;
                        var culture = System.Globalization.CultureInfo.CreateSpecificCulture("en-GB");
                        // Attempt to parse each part of the vertice
                        var success = float.TryParse(vertparts[0], style, culture, out vec.X);
                        success |= float.TryParse(vertparts[1], style, culture, out vec.Y);
                        success |= float.TryParse(vertparts[2], style, culture, out vec.Z);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing normal: {0}", line);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error parsing normal: {0}", line);
                    }

                    normals.Add(vec);
                }
                else if (line.StartsWith("f ")) // Face definition
                {
                    // Cut off beginning of line
                    var temp = line.Substring(2);

                    var face = new Tuple<TempVertex, TempVertex, TempVertex>(new TempVertex(), new TempVertex(), new TempVertex());

                    if (temp.Trim().Count(c => c == ' ') == 2) // Check if there's enough elements for a face
                    {
                        var faceparts = temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        int v1, v2, v3;
                        int t1, t2, t3;
                        int n1, n2, n3;

                        // Attempt to parse each part of the face
                        var success = int.TryParse(faceparts[0].Split('/')[0], out v1);
                        success |= int.TryParse(faceparts[1].Split('/')[0], out v2);
                        success |= int.TryParse(faceparts[2].Split('/')[0], out v3);

                        if (faceparts[0].Count(c => c == '/') >= 2)
                        {
                            success |= int.TryParse(faceparts[0].Split('/')[1], out t1);
                            success |= int.TryParse(faceparts[1].Split('/')[1], out t2);
                            success |= int.TryParse(faceparts[2].Split('/')[1], out t3);
                            success |= int.TryParse(faceparts[0].Split('/')[2], out n1);
                            success |= int.TryParse(faceparts[1].Split('/')[2], out n2);
                            success |= int.TryParse(faceparts[2].Split('/')[2], out n3);
                        }
                        else
                        {
                            if (texs.Count > v1 && texs.Count > v2 && texs.Count > v3)
                            {
                                t1 = v1;
                                t2 = v2;
                                t3 = v3;
                            }
                            else
                            {
                                t1 = 0;
                                t2 = 0;
                                t3 = 0;
                            }


                            if (normals.Count > v1 && normals.Count > v2 && normals.Count > v3)
                            {
                                n1 = v1;
                                n2 = v2;
                                n3 = v3;
                            }
                            else
                            {
                                n1 = 0;
                                n2 = 0;
                                n3 = 0;
                            }
                        }


                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Console.WriteLine("Error parsing face: {0}", line);
                        }
                        else
                        {
                            var tv1 = new TempVertex(v1, n1, t1);
                            var tv2 = new TempVertex(v2, n2, t2);
                            var tv3 = new TempVertex(v3, n3, t3);
                            face = new Tuple<TempVertex, TempVertex, TempVertex>(tv1, tv2, tv3);
                            faces.Add(face);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error parsing face: {0}", line);
                    }
                }
            }

            // Create the ObjVolume
            var vol = new ObjVolume();

            foreach (var face in faces)
            {
                var v1 = new FaceVertex(vertices[face.Item1.Vertex], normals[face.Item1.Normal], texs[face.Item1.TexCoords]);
                var v2 = new FaceVertex(vertices[face.Item2.Vertex], normals[face.Item2.Normal], texs[face.Item2.TexCoords]);
                var v3 = new FaceVertex(vertices[face.Item3.Vertex], normals[face.Item3.Normal], texs[face.Item3.TexCoords]);

                vol._faces.Add(new Tuple<FaceVertex, FaceVertex, FaceVertex>(v1, v2, v3));
            }

            return vol;
        }

        private class TempVertex
        {
            public readonly int Vertex;
            public readonly int Normal;
            public readonly int TexCoords;

            public TempVertex(int vertex = 0, int norm = 0, int tex = 0)
            {
                Vertex = vertex;
                Normal = norm;
                TexCoords = tex;
            }
        }

        public override Vector3[] GetNormals()
        {
            if (base.GetNormals().Length > 0)
            {
                return base.GetNormals();
            }

            List<Vector3> normals = new List<Vector3>();

            foreach (var face in _faces)
            {
                normals.Add(face.Item1.Normal);
                normals.Add(face.Item2.Normal);
                normals.Add(face.Item3.Normal);
            }

            foreach (var child in Children)
            {
                normals.AddRange(child.GetNormals());
            }

            return normals.ToArray();
        }

        public override int NormalCount => _faces.Count * 3;
    }

    internal class FaceVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Vector2 TextureCoords;

        public FaceVertex(Vector3 pos, Vector3 norm, Vector2 texCoords)
        {
            Position = pos;
            Normal = norm;
            TextureCoords = texCoords;
        }
    }
}
