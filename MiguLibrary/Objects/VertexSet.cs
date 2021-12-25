using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace MiguLibrary.Objects
{
    public class VertexSet
    {
        public Vector3[] Positions;
        public Vector3[] Normals;
        public Vector2[] TextureCoordinates;
        public Color[] Colors;
        public VertexBone[] BoneInfluences;
        public uint[] Ids;

        public VertexSet(int vertexCount)
        {
            Positions = new Vector3[vertexCount];
            Normals = new Vector3[vertexCount];
            TextureCoordinates = new Vector2[vertexCount];
            Colors = new Color[vertexCount];
            BoneInfluences = new VertexBone[vertexCount];
            Ids = new uint[vertexCount];
        }

        public int GetVertexIndex(int vertexId)
        {
            return Array.IndexOf(Ids, vertexId);
        }

        public Vector4[] GetColorsAsVec4()
        {
            Vector4[] colors = new Vector4[Colors.Length];

            for(int i = 0; i < Colors.Length; i++)
                colors[i] = Colors[i].AsVector4();

            return colors;
        }
    }

    public class VertexBone
    {
        public byte Bone0, Bone1, Bone2, Bone3;
        public float Weight0, Weight1, Weight2, Weight3;

        public override string ToString()
        {
            return $"<({Bone0}, {Bone1}, {Bone2}, {Bone3}), ({Weight0}, {Weight1}, {Weight2}, {Weight3})>";
        }
    }
}
