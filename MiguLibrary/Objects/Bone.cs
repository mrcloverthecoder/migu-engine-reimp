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
        public Vector4 Position;
        public short ParentId;
        public short ChildId;
        // I don't like to do this but this seems to be important
        public int mUnk0;

        public Bone(string name, Matrix4x4 pose, Vector4 position, int unk0, short parentId, short childId)
        {
            Name = name;
            Pose = pose;
            Position = position;
            mUnk0 = unk0;
            ParentId = parentId;
            ChildId = childId;
        }

        public static Bone Read(EndianBinaryReader reader)
        {
            string name = reader.ReadString(StringBinaryFormat.FixedLength, 32);
            Matrix4x4 pose = reader.ReadMatrix4x4();
            Vector4 position = reader.ReadVector4();
            position *= 0.08f;
            position.Z *= -1.0f;
            // TEST.BMD has a slightly different structure here, but I'm not worried in supporting it
            int unk0 = reader.ReadInt32();
            short parentId = reader.ReadInt16();
            short childId = reader.ReadInt16();
            reader.SeekCurrent(0x08);

            return new Bone(name, pose, position, unk0, parentId, childId);
        }
    }
}
