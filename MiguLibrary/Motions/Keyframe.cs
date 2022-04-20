using System.Numerics;

namespace MiguLibrary.Motions
{
    public struct MorphKeyframe
    {
        public int Frame;
        public float Progress;
    }

    public struct Keyframe
    {
        public int Frame;
        public Vector4 Rotation;
        public Vector4 Position;
    }
}
