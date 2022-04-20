using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using MiguLibrary.IO;
using MiguLibrary.Textures;

namespace MiguLibrary.Objects
{
    // Wanted to name this Object but it conflicts with System.Object
    // so it's impractical since this is exposed to the user who's making their tools and
    // it would be an arse having it conflict with another class
    public class ObjectData
    {
        public int PhysicsNumber;
        public List<Material> Materials;
        public List<Bone> Bones;
        public List<VertexSet> VertexSets;
        public List<FaceSet> FaceSets;
        public List<Morph> Morphs;

        public Dictionary<string, TextureResource> Textures;

        public ObjectData()
        {
            PhysicsNumber = 1;
            Materials = new List<Material>();
            Bones = new List<Bone>();
            VertexSets = new List<VertexSet>();
            FaceSets = new List<FaceSet>();
            Morphs = new List<Morph>();
            Textures = new Dictionary<string, TextureResource>();
        }

        public static ObjectData Load(string path) => Load(path, null);

        public static ObjectData Load(string path, string texBasePathAdd)
        {
            ObjectData obj = new ObjectData();

            using (var reader = new EndianBinaryReader(File.Open(path, FileMode.Open), Encoding.GetEncoding(932), Endianness.LittleEndian))
            {
                obj.Load(reader, path, texBasePathAdd);
            }

            return obj;
        }

        private void Load(EndianBinaryReader reader, string path, string texBasePathAdd)
        {
            if (!reader.ReadSignature("B3D OBJECT DATA", 16))
                throw new Exception($"File provided is not B3D Object Data!");

            reader.SeekBegin(0x74);

            PhysicsNumber = reader.ReadInt32();

            reader.SeekBegin(0x94);

            int textureCount = reader.ReadInt32();

            reader.SeekCurrent(0x10);

            List<string> textureNames = new List<string>();
            for (int i = 0; i < textureCount; i++)
                textureNames.Add(reader.ReadString(StringBinaryFormat.FixedLength, 32));

            string texturePath = Path.GetDirectoryName(path) + "/" + (String.IsNullOrEmpty(texBasePathAdd) ? "" : texBasePathAdd) + "/PSP";
            for(int i = 0; i < textureCount; i++)
            {
                Textures[textureNames[i]] = TextureResource.Load(texturePath + "/" + Path.ChangeExtension(textureNames[i], "gim"));
            }

            foreach(string s in textureNames)
            {
                Console.WriteLine($"Tex name: {s}");
            }

            int materialCount = reader.ReadInt32();

            reader.SeekCurrent(0x10);

            // Read materials
            for(int i = 0; i < materialCount; i++)
            {
                reader.SeekCurrent(0x06);
                
                short texNameIndex = reader.ReadInt16();

                string textureName = "";
                bool hasDiffuseTexture = true;

                /*Console.WriteLine(texNameIndex);
                Console.WriteLine(textureNames[texNameIndex]);*/

                if (texNameIndex != -1)
                    textureName = textureNames[texNameIndex];
                else
                    hasDiffuseTexture = false;

                Color diffuseColor = reader.ReadColor();
                Color ambientColor = reader.ReadColor();
                Color specularColor = reader.ReadColor();

                Materials.Add(new Material(textureName, diffuseColor, ambientColor, specularColor, hasDiffuseTexture));
            }

            int boneCount = reader.ReadInt32();
            reader.SeekCurrent(0x10);

            for(int i = 0; i < boneCount; i++)
            {
                string name = reader.ReadString(StringBinaryFormat.FixedLength, 32);
                Matrix4x4 pose = reader.ReadMatrix4x4();
                Vector3 position = reader.ReadVector3(true);
                reader.SeekCurrent(0x04);
                // TEST.BMD has a slightly different structure here, but I'm not worried in supporting it
                int unk0 = reader.ReadInt32();
                short parentId = reader.ReadInt16();
                short childId = reader.ReadInt16();
                reader.SeekCurrent(0x08);

                Bones.Add(new Bone(name, pose, position, unk0, parentId, childId));
            }

            // This now would be the IK count, then 12 bytes and finally the IK struct, but I'll just skip it
            // Since theres no point in reading something that's not even used in the final game
            reader.SeekCurrent(0x10);
            // Theres an unknown value so I'm just skipping it for now
            reader.SeekCurrent(0x04);
            
            int vertexSetCount = reader.ReadInt32();

            for (int i = 0; i < vertexSetCount; i++)
            {
                reader.SeekCurrent(0x06);
                ushort vertexCount = reader.ReadUInt16();
                VertexSets.Add(new VertexSet(vertexCount));
                reader.SeekCurrent(0x0C);
            }

            reader.SeekCurrent(0x04);

            // Loop again to gather actual vert data now
            for (int i = 0; i < vertexSetCount; i++)
            {
                for (int j = 0; j < VertexSets[i].Positions.Length; j++)
                {
                    VertexSets[i].Positions[j] = reader.ReadVector3();
                    VertexSets[i].Positions[j] *= 0.08f;
                    VertexSets[i].Normals[j] = reader.ReadVector3();
                    VertexSets[i].TextureCoordinates[j] = reader.ReadVector2();
                    VertexSets[i].Colors[j] = reader.ReadColor();

                    VertexBone bone = new VertexBone();
                    bone.Bone0 = reader.ReadByte();
                    bone.Bone1 = reader.ReadByte();
                    bone.Bone2 = reader.ReadByte();
                    bone.Bone3 = reader.ReadByte();
                    bone.Weight0 = reader.ReadSingle();
                    bone.Weight1 = reader.ReadSingle();
                    bone.Weight2 = reader.ReadSingle();
                    bone.Weight3 = reader.ReadSingle();

                    VertexSets[i].BoneInfluences[j] = bone;
                    VertexSets[i].Ids[j] = reader.ReadUInt32();
                }
            }

            int faceSetCount = reader.ReadInt32();
            for (int i = 0; i < faceSetCount; i++)
            {
                reader.SeekCurrent(0x04);

                int idx = reader.ReadInt32();
                ushort faceCount = reader.ReadUInt16();

                reader.SeekCurrent(0x02);

                short faceBoneCount = reader.ReadInt16();

                reader.SeekCurrent(0x02);

                short matIndex = reader.ReadInt16();

                reader.SeekCurrent(0x06);

                FaceSets.Add(new FaceSet(faceCount, faceBoneCount, idx, matIndex));
            }

            reader.SeekCurrent(0x04);
            for (int i = 0; i < faceSetCount; i++)
            {
                for (int j = 0; j < FaceSets[i].FaceIndices.Length; j++)
                    FaceSets[i].FaceIndices[j] = reader.ReadUInt16();
                for (int j = 0; j < FaceSets[i].BoneIndices.Length; j++)
                    FaceSets[i].BoneIndices[j] = reader.ReadInt16();
            }

            int morphCount = reader.ReadInt32();
            reader.SeekCurrent(0x0C);

            for(int i = 0; i < morphCount; i++)
            {
                reader.SeekCurrent(0x04);
                string name = reader.ReadString(StringBinaryFormat.FixedLength, 32);
                int panel = reader.ReadInt32();
                int vertexCount = reader.ReadInt32();

                Morphs.Add(new Morph(name, panel, vertexCount));
            }

            reader.SeekCurrent(0x04);

            for(int i = 0; i < morphCount; i++)
            {
                for(int j = 0; j < Morphs[i].Morphs.Length; j++)
                {
                    int id = reader.ReadInt32();
                    Vector3 transform = reader.ReadVector3(true);

                    Morphs[i].Morphs[j] = new VertexMorph(id, transform);
                }
            }
        }
    }
}
