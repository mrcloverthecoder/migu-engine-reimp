using System;
using System.Collections.Generic;
using System.Linq;
using Sn = System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Threading;
using OpenTK;

namespace MiguModelViewer
{
    public class Utils
    {
        public static string DecimalSeparator = Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator;

        public static float ParseFloatEx(string str)
        {
            return float.Parse(str.Replace(".", DecimalSeparator));
        }

        public static string FloatToStringEx(float f)
        {
            return f.ToString().Replace(DecimalSeparator, ".");
        }

        public static string Vec3StringEx(Sn.Vector3 v)
        {
            return $"{FloatToStringEx(v.X)},{FloatToStringEx(v.Y)},{FloatToStringEx(v.Z)}";
        }

        public static Sn.Vector3 StringVec3Ex(string s)
        {
            string[] values = s.Split(',');

            return new Sn.Vector3(ParseFloatEx(values[0]), ParseFloatEx(values[1]), ParseFloatEx(values[2]));
        }

        /// <summary>
        ///     Linearly interpolate between two points.
        /// </summary>
        /// <param name="p1">Starting point</param>
        /// <param name="p2">Destination point</param>
        /// <param name="f1">Starting time</param>
        /// <param name="f2">Destination time</param>
        /// <param name="f">Current time</param>
        /// <returns>Interpolated point</returns>
        public static Vector3 Lerp(Vector3 p1, Vector3 p2, float f1, float f2, float f)
        {
            float progress = (f - f1) / (f2 - f1);
            Vector3 pointDelta = p2 - p1;
            return p1 + pointDelta * progress;
        }

        public static Vector4 Lerp(Vector4 p1, Vector4 p2, float f1, float f2, float f)
        {
            float progress = (f - f1) / (f2 - f1);
            Vector4 pointDelta = p2 - p1;
            return p1 + pointDelta * progress;
        }

        public static float Lerp(float p1, float p2, float f1, float f2, float f)
        {
            float progress = (f - f1) / (f2 - f1);
            float pointDelta = p2 - p1;
            return p1 + pointDelta * progress;
        }

        public static Vector2 Normalize(Vector2 pos, Vector2 max)
        {
            return new Vector2(pos.X / max.X, pos.Y / max.Y);
        }
    }
}
