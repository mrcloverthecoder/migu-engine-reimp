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
        public static Matrix4 ToGL (this Sn.Matrix4x4 m)
        {
            return new Matrix4(m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);
        } 

        public static Vector3 ToGL(this Sn.Vector3 v)
        {
            return new Vector3(v.X, v.Y, v.Z);
        }
    }
}
