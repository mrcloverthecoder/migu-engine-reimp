using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace MiguModelViewer
{
    public class Cache
    {
        public static Dictionary<string, bool> TextureAlpha = new Dictionary<string, bool>();

        public static void WriteTextureAlphaCache()
        {
            List<string> lines = new List<string>();

            foreach(KeyValuePair<string, bool> pair in TextureAlpha)
            {
                lines.Add($"{pair.Key} {pair.Value}");
            }

            if (!Directory.Exists("cache/"))
                Directory.CreateDirectory("cache/");

            File.WriteAllLines("cache/texture_alpha.cache", lines);
        }

        public static void ReadTextureAlphaCache()
        {
            TextureAlpha.Clear();

            if(File.Exists("cache/texture_alpha.cache"))
            {
                string[] lines = File.ReadAllLines("cache/texture_alpha.cache");

                foreach (string line in lines)
                    TextureAlpha[line.Split(' ')[0]] = line.Split(' ')[1] == "True";
            }
        }
    }

    public class State
    {
        public static int SceneId = 1;
    }

    public class Config
    {
        public static string DataPath = "";

        public static float Width = 960.0f;
        public static float Height = 540.0f;

        public static float Framerate = 60.0f;
    }
}
