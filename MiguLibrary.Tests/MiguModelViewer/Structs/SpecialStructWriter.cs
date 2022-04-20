using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiguModelViewer.Structs
{
    public class SpecialStructWriter
    {
        public static void WriteLightSetting(LightSetting setting, string path) =>
            WriteLightSetting(new LightSetting[1] { setting }, path);

        public static void WriteLightSetting(LightSetting[] settings, string path)
        {
            StringBuilder builder = new StringBuilder();

            foreach(LightSetting set in settings)
            {
                builder.Append($"{(int)set.Type}\t{Utils.Vec3StringEx(set.Color)}\t{Utils.FloatToStringEx(set.Intensity)}\n");
            }

            File.WriteAllText(path, builder.ToString());
        }
    }
}
