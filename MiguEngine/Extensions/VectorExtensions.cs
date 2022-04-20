using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GL = OpenTK;
using Sn = System.Numerics;

namespace MiguEngine.Extensions
{
    public static unsafe class VectorExtensions
    {
        public static GL.Vector2 ToGL(this Sn.Vector2 v) => *(GL.Vector2*)&v;
        public static GL.Vector3 ToGL(this Sn.Vector3 v) => *(GL.Vector3*)&v;
        public static GL.Vector4 ToGL(this Sn.Vector4 v) => *(GL.Vector4*)&v;

        public static GL.Matrix4 ToGL(this Sn.Matrix4x4 m) => *(GL.Matrix4*)&m;

        public static GL.Vector3 ToEulerAngles(this GL.Quaternion q)
        {
            GL.Vector3 eulerAngles;

            // Threshold for the singularities found at the north/south poles.
            const float SINGULARITY_THRESHOLD = 0.4999995f;

            var sqw = q.W * q.W;
            var sqx = q.X * q.X;
            var sqy = q.Y * q.Y;
            var sqz = q.Z * q.Z;
            var unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            var singularityTest = (q.X * q.Z) + (q.W * q.Y);

            if (singularityTest > SINGULARITY_THRESHOLD * unit)
            {
                eulerAngles.Z = (float)(2 * Math.Atan2(q.X, q.W));
                eulerAngles.Y = GL.MathHelper.PiOver2;
                eulerAngles.X = 0;
            }
            else if (singularityTest < -SINGULARITY_THRESHOLD * unit)
            {
                eulerAngles.Z = (float)(-2 * Math.Atan2(q.X, q.W));
                eulerAngles.Y = -GL.MathHelper.PiOver2;
                eulerAngles.X = 0;
            }
            else
            {
                eulerAngles.Z = (float)Math.Atan2(2 * ((q.W * q.Z) - (q.X * q.Y)), sqw + sqx - sqy - sqz);
                eulerAngles.Y = (float)Math.Asin(2 * singularityTest / unit);
                eulerAngles.X = (float)Math.Atan2(2 * ((q.W * q.X) - (q.Y * q.Z)), sqw - sqx - sqy + sqz);
            }

            return eulerAngles;
        }
    }
}
