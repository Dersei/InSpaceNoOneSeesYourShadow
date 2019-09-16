using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using InSpaceNoOneSeesYourShadow.Objects3D.Shapes;
using OpenTK;

namespace InSpaceNoOneSeesYourShadow.Content
{
    internal static class ModelLoader
    {
        private static readonly Dictionary<string, ObjVolume> Cache = new Dictionary<string, ObjVolume>();

        private static bool CheckIfCached(string name, out ObjVolume obj)
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
        /// <returns>ObjVolume of loaded model</returns>
        public static ObjVolume LoadFromFile(string filename)
        {
            if (CheckIfCached(filename, out var result))
            {
                return result;
            }

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

            Cache.Add(filename, obj);

            return obj;
        }


        private static ObjVolume LoadFromString(string obj)
        {
            // Separate lines from the file
            var lines = new List<string>(obj.Split('\n'));

            // Lists to hold model data
            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var texs = new List<Vector2>();
            var faces = new List<Tuple<ObjVolume.TempVertex, ObjVolume.TempVertex, ObjVolume.TempVertex>>();

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

                    var face = new Tuple<ObjVolume.TempVertex, ObjVolume.TempVertex, ObjVolume.TempVertex>(new ObjVolume.TempVertex(), new ObjVolume.TempVertex(), new ObjVolume.TempVertex());

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
                            var tv1 = new ObjVolume.TempVertex(v1, n1, t1);
                            var tv2 = new ObjVolume.TempVertex(v2, n2, t2);
                            var tv3 = new ObjVolume.TempVertex(v3, n3, t3);
                            face = new Tuple<ObjVolume.TempVertex, ObjVolume.TempVertex, ObjVolume.TempVertex>(tv1, tv2, tv3);
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

                vol._faces.Add((v1, v2, v3));
            }

            return vol;
        }
    }
}
