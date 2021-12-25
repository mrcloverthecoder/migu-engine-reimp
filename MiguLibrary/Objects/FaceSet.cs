using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiguLibrary.Objects
{
    public class FaceSet
    {
        public int Index;
        public int MaterialIndex;
        public ushort[] FaceIndices;
        public short[] BoneIndices;

        public FaceSet(ushort faceCount, short boneCount)
        {
            Index = 0;
            MaterialIndex = 0;
            FaceIndices = new ushort[faceCount];
            BoneIndices = new short[boneCount];
        }

        public FaceSet(ushort faceCount, short boneCount, int idx)
        {
            Index = idx;
            MaterialIndex = 0;
            FaceIndices = new ushort[faceCount];
            BoneIndices = new short[boneCount];
        }

        public FaceSet(ushort faceCount, short boneCount, int idx, short matIdx)
        {
            Index = idx;
            MaterialIndex = matIdx;
            FaceIndices = new ushort[faceCount];
            BoneIndices = new short[boneCount];
        }
    }
}
