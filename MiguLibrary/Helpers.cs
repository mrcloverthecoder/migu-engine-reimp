using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Numerics;

namespace MiguLibrary
{
    public static class VectorHelper
    {
        public static float[] ToFloatArray(Vector4[] v)
        {
            float[] array = new float[v.Length * 4];

            int position = 0;
            for(int i = 0; i < v.Length; i++)
            {
                array[position] = v[i].X;
                array[position + 1] = v[i].Y;
                array[position + 2] = v[i].Z;
                array[position + 3] = v[i].W;

                position += 4;
            }

            return array;
        }
    }

    public static class MathHelper
    {
        public static float Rad2Deg = (float)(Math.PI * 2.0f) / 360.0f;

        public static Vector3 FromQ2(Quaternion q1)
        {
            float sqw = q1.W * q1.W;
            float sqx = q1.X * q1.X;
            float sqy = q1.Y * q1.Y;
            float sqz = q1.Z * q1.Z;
            float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            float test = q1.X * q1.W - q1.Y * q1.Z;
            Vector3 v;

            if (test > 0.4995f * unit)
            { // singularity at north pole
                v.Y = (float)(2f * Math.Atan2(q1.Y, q1.X));
                v.X = (float)Math.PI / 2.0f;
                v.Z = 0;
                return NormalizeAngles(v * Rad2Deg);
            }
            if (test < -0.4995f * unit)
            { // singularity at south pole
                v.Y = (float)(-2f * Math.Atan2(q1.Y, q1.X));
                v.X = (float)-Math.PI / 2f;
                v.Z = 0;
                return NormalizeAngles(v * Rad2Deg);
            }
            Quaternion q = new Quaternion(q1.W, q1.Z, q1.X, q1.Y);
            v.Y = (float)Math.Atan2(2f * q.X * q.W + 2f * q.Y * q.Z, 1 - 2f * (q.Z * q.Z + q.W * q.W));     // Yaw
            v.X = (float)Math.Asin(2f * (q.X * q.Z - q.W * q.Y));                             // Pitch
            v.Z = (float)Math.Atan2(2f * q.X * q.Y + 2f * q.Z * q.W, 1 - 2f * (q.Y * q.Y + q.Z * q.Z));      // Roll
            return NormalizeAngles(v * Rad2Deg);
        }

        static Vector3 NormalizeAngles(Vector3 angles)
        {
            angles.X = NormalizeAngle(angles.X);
            angles.Y = NormalizeAngle(angles.Y);
            angles.Z = NormalizeAngle(angles.Z);
            return angles;
        }

        static float NormalizeAngle(float angle)
        {
            while (angle > 360)
                angle -= 360;
            while (angle < 0)
                angle += 360;
            return angle;
        }

        public static Vector2 Normalize(Vector2 pos, Vector2 max)
        {
            return new Vector2(pos.X / max.X, pos.Y / max.Y);
        }

        public static int RoundUp(int value, int multiple)
        {
            // Return the same number if it is already a multiple
            if (value % multiple == 0)
                return value;

            return value + (multiple - (value % multiple));
        }

        public static uint RoundUp(uint value, int multiple)
        {
            // Return the same number if it is already a multiple
            if (value % multiple == 0)
                return value;

            return value + ((uint)multiple - (value % (uint)multiple));
        }

        /// <summary>
        /// Evaluate whether a given integral value is a power of 2.
        /// </summary>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsPow2(int value) => (value & (value - 1)) == 0 && value > 0;
    }
}
