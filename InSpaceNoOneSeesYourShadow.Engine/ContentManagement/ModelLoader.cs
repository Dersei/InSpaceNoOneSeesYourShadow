using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using InSpaceNoOneSeesYourShadow.Engine.Objects3D.Shapes;
using InSpaceNoOneSeesYourShadow.Engine.Utils;
using OpenTK;

namespace InSpaceNoOneSeesYourShadow.Engine.ContentManagement
{
    public static class ModelLoader
    {
        private static readonly Dictionary<string, Model> Cache = new Dictionary<string, Model>();

        private static bool CheckIfCached(string name, out Model obj)
        {
            if (Cache.ContainsKey(name))
            {
                obj = Cache[name];
                return true;
            }

            obj = default;
            return false;
        }
        /// <summary>
        /// Loads a model from a file.
        /// </summary>
        /// <param name="filename">File to load model from</param>
        /// <returns>Model of loaded model</returns>
        public static Model LoadFromFile(string filename)
        {
            if (CheckIfCached(filename, out var result))
            {
                return result;
            }

            var obj = new Model();
            try
            {
                using var reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read));
                obj = LoadFromString(reader.ReadToEnd());
            }
            catch (FileNotFoundException)
            {
                Logger.LogConsole($@"File not found: {filename}");
            }
            catch (Exception)
            {
                Logger.LogConsole($@"Error loading file: {filename}");
            }

            Cache.Add(filename, obj);

            return obj;
        }


        private static Model LoadFromString(string obj)
        {
            const NumberStyles style = NumberStyles.Number;
            var culture = CultureInfo.CreateSpecificCulture("en-GB");
            // Separate lines from the file
            var lines = new List<string>(obj.Split('\n'));

            // Lists to hold model data
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var texs = new List<Vector2>();
            var faces = new List<(TempVertex, TempVertex, TempVertex)>();

            // Base values
            vertices.Add(new Vector3());
            texs.Add(new Vector2());
            normals.Add(new Vector3());

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
                        var success = float.TryParse(vertparts[0], style, culture, out vec.X);
                        success |= float.TryParse(vertparts[1], style, culture, out vec.Y);
                        success |= float.TryParse(vertparts[2], style, culture, out vec.Z);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Logger.LogConsole($"Error parsing vertex: {line}");
                        }
                    }
                    else
                    {
                        Logger.LogConsole($"Error parsing vertex: {line}");
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
                        var success = float.TryParse(texcoordparts[0], style, culture, out vec.X);
                        success |= float.TryParse(texcoordparts[1], style, culture, out vec.Y);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Logger.LogConsole($"Error parsing texture coordinate: {line}");
                        }
                    }
                    else
                    {
                        Logger.LogConsole($"Error parsing texture coordinate: {line}");
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

                        // Attempt to parse each part of the vertice
                        var success = float.TryParse(vertparts[0], style, culture, out vec.X);
                        success |= float.TryParse(vertparts[1], style, culture, out vec.Y);
                        success |= float.TryParse(vertparts[2], style, culture, out vec.Z);

                        // If any of the parses failed, report the error
                        if (!success)
                        {
                            Logger.LogConsole($"Error parsing normal: {line}");
                        }
                    }
                    else
                    {
                        Logger.LogConsole($"Error parsing normal: {line}");
                    }

                    normals.Add(vec);
                }
                else if (line.StartsWith("f ")) // Face definition
                {
                    // Cut off beginning of line
                    var temp = line.Substring(2);

                    if (temp.Trim().Count(c => c == ' ') == 2) // Check if there's enough elements for a face
                    {
                        var faceParts = temp.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                        int t1, t2, t3;
                        int n1, n2, n3;

                        // Attempt to parse each part of the face
                        var success = int.TryParse(faceParts[0].Split('/')[0], out var v1);
                        success |= int.TryParse(faceParts[1].Split('/')[0], out var v2);
                        success |= int.TryParse(faceParts[2].Split('/')[0], out var v3);

                        if (faceParts[0].Count(c => c == '/') >= 2)
                        {
                            success |= int.TryParse(faceParts[0].Split('/')[1], out t1);
                            success |= int.TryParse(faceParts[1].Split('/')[1], out t2);
                            success |= int.TryParse(faceParts[2].Split('/')[1], out t3);
                            success |= int.TryParse(faceParts[0].Split('/')[2], out n1);
                            success |= int.TryParse(faceParts[1].Split('/')[2], out n2);
                            success |= int.TryParse(faceParts[2].Split('/')[2], out n3);
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
                            Logger.LogConsole($"Error parsing face: {line}");
                        }
                        else
                        {
                            var tv1 = new TempVertex(v1, n1, t1);
                            var tv2 = new TempVertex(v2, n2, t2);
                            var tv3 = new TempVertex(v3, n3, t3);
                            faces.Add((tv1, tv2, tv3));
                        }
                    }
                    else
                    {
                        Logger.LogConsole($"Error parsing face: {line}");
                    }
                }
            }

            // Create the Model
            var vol = new Model();

            foreach (var face in faces)
            {
                var v1 = new Model.FaceVertex(vertices[face.Item1.Vertex], normals[face.Item1.Normal], texs[face.Item1.TexCoords]);
                var v2 = new Model.FaceVertex(vertices[face.Item2.Vertex], normals[face.Item2.Normal], texs[face.Item2.TexCoords]);
                var v3 = new Model.FaceVertex(vertices[face.Item3.Vertex], normals[face.Item3.Normal], texs[face.Item3.TexCoords]);

                vol.Faces.Add((v1, v2, v3));
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

    }
}
