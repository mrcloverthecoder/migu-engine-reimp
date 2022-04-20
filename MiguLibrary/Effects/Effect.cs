using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace MiguLibrary.Effects
{
    public struct Effect
    {
        public string Name; // 0x00
        public int Id;      // 0x10
        public Vector3 Translation; // 0x28; Translation/end position values? I don't quite know
        public float Distance; // 0x34; Distance between each particle?
        public TextureCoordinates TexCoord; // 0x50
        public Vector3 Rotation; // 0x78
        public Vector3 Scale; // 0x88
    }

    public struct TextureCoordinates
    {
        public float X;
        public float Y;
        public float Width;
        public float Height;
    }
}
