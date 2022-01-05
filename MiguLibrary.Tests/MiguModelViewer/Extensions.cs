using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sn = System.Numerics;
using OpenTK;

namespace MiguModelViewer
{
    public static class Extensions
    {
        public static int PushString(this char[] array, string str, int position)
        {
            for(int i = 0; i < str.Length; i++)
            {
                array[position + i] = str[i];
            }

            return position + str.Length;
        }

        public static Quaternion ToGL(this Sn.Quaternion q)
        {
            return new Quaternion(q.X, q.Y, q.Z, q.W);
        }
        public static Matrix4 ToGL (this Sn.Matrix4x4 m)
        {
            return new Matrix4(m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);
        } 

        public static Vector3 ToGL(this Sn.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }

        public static Sn.Vector3 ToNumerics(this Vector3 v)
        {
            return new Sn.Vector3(v.X, v.Y, v.Z);
        }

        public static Vector4 ToGL(this Sn.Vector4 v)
        {
            return new Vector4(v.X, v.Y, v.Z, v.W);
        }

        public static Sn.Vector4 ToNumerics(this Vector4 v)
        {
            return new Sn.Vector4(v.X, v.Y, v.Z, v.W);
        }
    }
}
