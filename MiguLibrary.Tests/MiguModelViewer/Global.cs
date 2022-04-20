using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using MiguModelViewer.Structs;

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

    public class Lists
    {
        public static FileListInfo[] MotionList;

        static Lists()
        {
            MotionList = SpecialStructReader.ReadFileList("Resource/MotionList.txt");
        }
    }

    public class Tables
    {
        public static CostumeTable[] Costumes;

        static Tables()
        {
            Costumes = TableLoader.LoadCosTable("Resource/CostumeTable.txt");
        }
    }

    public class State
    {
        public static int SelectedGumi = 14;
        public static int SceneId = 29;

        public static int Points = 0;
    }

    public class Config
    {
        public static string DataPath = "";

        public static float Width = 960.0f;
        public static float Height = 540.0f;

        public static float Framerate = 60.0f;
        public static float Delta { get => Framerate / 30.0f; }
        public static float InverseDelta { get => 30.0f / Framerate; }

        // For 2d
        public static float ScaleX { get => Width / 480.0f; }
        public static float ScaleY { get => Height / 272.0f; }
    }
}
