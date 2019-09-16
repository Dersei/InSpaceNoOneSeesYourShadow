using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using InSpaceNoOneSeesYourShadow.Objects3D;
using OpenTK;

namespace InSpaceNoOneSeesYourShadow.Content
{
    internal static class MaterialLoader
    {
        private static readonly Dictionary<string, Dictionary<string, Material>> Cache = new Dictionary<string, Dictionary<string, Material>>();

        private static bool CheckIfCached(string name, out Dictionary<string, Material> obj)
        {
            if (Cache.ContainsKey(name))
            {
                obj = Cache[name];
                return true;
            }

            obj = default;
            return false;
        }

        public static Dictionary<string, Material> LoadFromFile(string filename)
        {
            if (CheckIfCached(filename, out var result))
            {
                return result;
            }

            var mats = new Dictionary<string, Material>();

            try
            {
                var currentmat = "";
                using (var reader = new StreamReader(new FileStream(filename, FileMode.Open, FileAccess.Read)))
                {
                    string currentLine;

                    while (!reader.EndOfStream)
                    {
                        currentLine = reader.ReadLine();

                        if (!currentLine.StartsWith("newmtl"))
                        {
                            if (currentmat.StartsWith("newmtl"))
                            {
                                currentmat += currentLine + "\n";
                            }
                        }
                        else
                        {
                            if (currentmat.Length > 0)
                            {
                                var newMat = new Material();

                                newMat = LoadFromString(currentmat, out var newMatName);

                                mats.Add(newMatName, newMat);
                            }

                            currentmat = currentLine + "\n";
                        }
                    }
                }

                // Add final material
                if (currentmat.Count(c => c == '\n') > 0)
                {
                    var newMat = new Material();

                    newMat = LoadFromString(currentmat, out var newMatName);

                    mats.Add(newMatName, newMat);
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine("File not found: {0}", filename);
            }
            catch (Exception)
            {
                Console.WriteLine("Error loading file: {0}", filename);
            }

            Cache.Add(filename, mats);

            return mats;
        }

        public static Material LoadFromString(string mat, out string name)
        {
            var output = new Material();
            name = "";

            var lines = mat.Split('\n').ToList();

            // Skip until the material definition starts
            lines = lines.SkipWhile(s => !s.StartsWith("newmtl ")).ToList();

            // Make sure an actual material was included
            if (lines.Count != 0)
            {
                // Get name from first line
                name = lines[0].Substring("newmtl ".Length);
            }

            // Remove leading whitespace
            lines = lines.Select(s => s.Trim()).ToList();

            // Read material properties
            foreach (var line in lines)
            {
                // Skip comments and blank lines
                if (line.Length < 3 || line.StartsWith("//") || line.StartsWith("#"))
                {
                    continue;
                }

                // Parse ambient color
                if (line.StartsWith("Ka"))
                {
                    var colorparts = line.Substring(3).Split(' ');

                    // Check that all vector fields are present
                    if (colorparts.Length < 3)
                    {
                        throw new ArgumentException("Invalid color data");
                    }

                    var vec = new Vector3();
                    var style = NumberStyles.Number;
                    var culture = CultureInfo.CreateSpecificCulture("en-GB");
                    // Attempt to parse each part of the color
                    var success = float.TryParse(colorparts[0], style, culture, out vec.X);
                    success |= float.TryParse(colorparts[1], style, culture, out vec.Y);
                    success |= float.TryParse(colorparts[2], style, culture, out vec.Z);

                    output.AmbientColor = new Vector3(float.Parse(colorparts[0], style, culture), float.Parse(colorparts[1], style, culture), float.Parse(colorparts[2], style, culture));

                    // If any of the parses failed, report the error
                    if (!success)
                    {
                        Console.WriteLine("Error parsing color: {0}", line);
                    }
                }

                // Parse diffuse color
                if (line.StartsWith("Kd"))
                {
                    var colorparts = line.Substring(3).Split(' ');

                    // Check that all vector fields are present
                    if (colorparts.Length < 3)
                    {
                        throw new ArgumentException("Invalid color data");
                    }

                    var vec = new Vector3();

                    // Attempt to parse each part of the color
                    var style = NumberStyles.Number;
                    var culture = CultureInfo.CreateSpecificCulture("en-GB");
                    var success = float.TryParse(colorparts[0], style, culture, out vec.X);
                    success |= float.TryParse(colorparts[1], style, culture, out vec.Y);
                    success |= float.TryParse(colorparts[2], style, culture, out vec.Z);
                    output.DiffuseColor = new Vector3(float.Parse(colorparts[0], style, culture), float.Parse(colorparts[1], style, culture), float.Parse(colorparts[2], style, culture));

                    // If any of the parses failed, report the error
                    if (!success)
                    {
                        Console.WriteLine("Error parsing color: {0}", line);
                    }
                }

                // Parse specular color
                if (line.StartsWith("Ks"))
                {
                    var colorparts = line.Substring(3).Split(' ');

                    // Check that all vector fields are present
                    if (colorparts.Length < 3)
                    {
                        throw new ArgumentException("Invalid color data");
                    }

                    var vec = new Vector3();

                    // Attempt to parse each part of the color
                    var style = NumberStyles.Number;
                    var culture = CultureInfo.CreateSpecificCulture("en-GB");
                    var success = float.TryParse(colorparts[0], style, culture, out vec.X);
                    success |= float.TryParse(colorparts[1], style, culture, out vec.Y);
                    success |= float.TryParse(colorparts[2], style, culture, out vec.Z);

                    output.SpecularColor = new Vector3(float.Parse(colorparts[0], style, culture), float.Parse(colorparts[1], style, culture), float.Parse(colorparts[2], style, culture));

                    // If any of the parses failed, report the error
                    if (!success)
                    {
                        Console.WriteLine("Error parsing color: {0}", line);
                    }
                }

                // Parse specular exponent
                if (line.StartsWith("Ns"))
                {
                    // Attempt to parse each part of the color
                    const NumberStyles style = NumberStyles.Number;
                    var culture = CultureInfo.CreateSpecificCulture("en-GB");
                    var success = float.TryParse(line.Substring(3), style, culture, out var exponent);

                    output.SpecularExponent = exponent;

                    // If any of the parses failed, report the error
                    if (!success)
                    {
                        Console.WriteLine("Error parsing specular exponent: {0}", line);
                    }
                }

                // Parse ambient map
                if (line.StartsWith("map_Ka"))
                {
                    // Check that file name is present
                    if (line.Length > "map_Ka".Length + 6)
                    {
                        output.AmbientMap = line.Substring("map_Ka".Length + 1);
                    }
                }

                // Parse diffuse map
                if (line.StartsWith("map_Kd"))
                {
                    // Check that file name is present
                    if (line.Length > "map_Kd".Length + 6)
                    {
                        output.DiffuseMap = line.Substring("map_Kd".Length + 1);
                    }
                }

                // Parse specular map
                if (line.StartsWith("map_Ks"))
                {
                    // Check that file name is present
                    if (line.Length > "map_Ks".Length + 6)
                    {
                        output.SpecularMap = line.Substring("map_Ks".Length + 1);
                    }
                }

                // Parse normal map
                if (line.StartsWith("map_normal"))
                {
                    // Check that file name is present
                    if (line.Length > "map_normal".Length + 6)
                    {
                        output.NormalMap = line.Substring("map_normal".Length + 1);
                    }
                }

                // Parse opacity map
                if (line.StartsWith("map_opacity"))
                {
                    // Check that file name is present
                    if (line.Length > "map_opacity".Length + 6)
                    {
                        output.OpacityMap = line.Substring("map_opacity".Length + 1);
                    }
                }

            }

            return output;
        }
    }
}
