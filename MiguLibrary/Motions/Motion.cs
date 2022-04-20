using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MiguLibrary.IO;

namespace MiguLibrary.Motions
{
    public class Motion
    {
        public string Name;
        public List<MotionBone> Bones;
        public List<MotionMorph> Morphs;

        public Motion()
        {
            Name = String.Empty;
            Bones = new List<MotionBone>();
            Morphs = new List<MotionMorph>();
        }

        public void Read(EndianBinaryReader reader)
        {
            if (!reader.ReadSignature("B3D_ANM", 8))
                throw new Exception("Not a BMM file!");

            reader.SeekCurrent(12);

            int motionCount = reader.ReadInt32();   // Bones
            int skinCount = reader.ReadInt32();     // Morphs
            int materialCount = reader.ReadInt32(); // Material animations

            bool hasMotion = motionCount > 0;
            bool hasSkin = skinCount > 0;
            bool hasMtr = materialCount > 0;

            if(hasMotion)
            {
                if (!reader.ReadSignature("B3D_MTN", 0x08))
                    throw new Exception("Invalid structure");

                reader.SeekCurrent(0x10);

                int boneCount = reader.ReadInt32();

                reader.SeekCurrent(0x04);

                List<int> frameCountCache = new List<int>();

                for(int i = 0; i < boneCount; i++)
                {
                    MotionBone bone = new MotionBone();

                    bone.Name = reader.ReadString(StringBinaryFormat.FixedLength, 32);

                    frameCountCache.Add(reader.ReadInt32());

                    bone.Keyframes = new Keyframe[frameCountCache.Last()];

                    bone.Index = reader.ReadInt32();

                    reader.SeekCurrent(0x04);

                    bone.Type = (MotionBoneType)reader.ReadInt32();

                    reader.SeekCurrent(0x08);

                    Bones.Add(bone);
                }

                // Re looping
                for(int i = 0; i < boneCount; i++)
                {
                    for (int j = 0; j < frameCountCache[i]; j++)
                    {
                        Keyframe key = new Keyframe();

                        key.Frame = j;

                        if (Bones[i].Type == MotionBoneType.Rot)
                        {
                            key.Rotation = reader.ReadVector4();
                        }
                        else if(Bones[i].Type == MotionBoneType.RotTrans)
                        {
                            // Read rotation at its offset first
                            reader.ReadAtOffset(reader.Position + (0x10 * frameCountCache[i]), () =>
                            {
                                key.Rotation = reader.ReadVector4();
                            });

                            key.Position = reader.ReadVector4(2);
                        }

                        Bones[i].Keyframes[j] = key;
                    }

                    // Jump the rotation data
                    if (Bones[i].Type == MotionBoneType.RotTrans)
                    {
                        reader.SeekCurrent(0x10 * frameCountCache[i]);

                    }
                }
            }

            // Morph
            if(hasSkin)
            {
                if (!reader.ReadSignature("B3D_SKN", 0x08))
                    throw new Exception("Invalid structure");

                reader.SeekCurrent(0x0C);
                int morphCount = reader.ReadInt32();
                reader.SeekCurrent(0x08);

                for(int i = 0; i < morphCount; i++)
                {
                    MotionMorph morph = new MotionMorph();
                    morph.Name = reader.ReadString(StringBinaryFormat.FixedLength, 0x10);
                    morph.Index = reader.ReadInt32();

                    int frameCount = reader.ReadInt32();
                    morph.Keyframes = new MorphKeyframe[frameCount];

                    reader.SeekCurrent(0x08);

                    Morphs.Add(morph);
                }

                // Re-looping to get the actual frame data
                for(int i = 0; i < morphCount; i++)
                {
                    for(int j = 0; j < Morphs[i].Keyframes.Length; j++)
                    {
                        MorphKeyframe key = new MorphKeyframe();

                        key.Frame = reader.ReadInt32();
                        key.Progress = reader.ReadSingle();

                        reader.SeekCurrent(0x08);

                        Morphs[i].Keyframes[j] = key;
                    }
                }
            }
        }

        public static Motion FromFile(string path)
        {
            Motion mot = new Motion();

            using(var reader = new EndianBinaryReader(File.Open(path, FileMode.Open), Encoding.GetEncoding(932), Endianness.LittleEndian))
            {
                mot.Read(reader);
            }

            mot.Name = Path.GetFileName(path).Split('.')[0].ToUpper();

            return mot;
        }
    }
}
