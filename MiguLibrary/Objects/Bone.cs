using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using MiguLibrary.IO;

namespace MiguLibrary.Objects
{
    public class Bone
    {
        public string Name;
        public Matrix4x4 Pose;
        public Vector3 Position;
        // For other purposes
        public Vector4 Rotation;
        public short ParentId;
        public short ChildId;
        // I don't like to do this but this seems to be important
        public int mUnk0;

        public Bone(string name, Matrix4x4 pose, Vector3 position, int unk0, short parentId, short childId)
        {
            Name = name;
            Pose = pose;
            Position = position;
            mUnk0 = unk0;
            ParentId = parentId;
            ChildId = childId;
            Rotation = Vector4.Zero;
        }
    }
}
