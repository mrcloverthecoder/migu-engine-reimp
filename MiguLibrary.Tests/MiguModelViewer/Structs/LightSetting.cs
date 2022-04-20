using System;
using System.IO;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace MiguModelViewer.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LightSetting
    {
        public int Id;
        public Vector3 Color;
        public float Intensity;
        public LightType Type;

        public LightSetting(Vector3 color, float intensity, int id = 0, LightType type = LightType.None)
        {
            Id = id;
            Color = color;
            Intensity = intensity;
            Type = type;
        }
    }

    public enum LightType : int
    {
        All = 0,
        Chara = 1,
        // Everything other than chara
        Prop = 2,
        None = 3 // Affects nothing
    }
}
