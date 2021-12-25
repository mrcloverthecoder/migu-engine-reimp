using System.Numerics;

namespace MiguLibrary
{
    public class Color
    {
        public static readonly Color White = new Color(255, 255, 255, 255);
        public static readonly Color Black = new Color(0, 0, 0, 0);

        public byte R;
        public byte G;
        public byte B;
        public byte A;

        public Color(byte r, byte g, byte b, byte a)
        {
            R = r;
            G = g;
            B = b;
            A = a;
        }

        public Color()
        {
            R = 255;
            G = 255;
            B = 255;
            A = 255;
        }

        public Vector4 AsVector4()
        {
            return new Vector4(R / 255.0f, G / 255.0f, B / 255.0f, A / 255.0f);
        }

        public override string ToString()
        {
            return $"{R} {G} {B} {A}";
        }
    }
}
