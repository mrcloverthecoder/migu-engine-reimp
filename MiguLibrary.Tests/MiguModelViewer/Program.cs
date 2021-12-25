using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiguModelViewer
{
    class Program
    {
        static void Main(string[] args)
        {
            // Load config
            string[] cfgLines = File.ReadAllLines("Config.txt");

            Config.DataPath = cfgLines[0];
            int width = int.Parse(cfgLines[2].Split('x')[0]);
            int height = int.Parse(cfgLines[2].Split('x')[1]);

            if (args.Length > 0)
            {
                State.SceneId = int.Parse(args[0]);
            }

            Config.Width = (float)width;
            Config.Height = (float)height;

            Cache.ReadTextureAlphaCache();

            using(Window w = new Window(width, height, "MiguModelViewer"))
            {
                w.Run(30.0);
            }
        }
    }
}
