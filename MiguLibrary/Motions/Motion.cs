using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiguLibrary.IO;

namespace MiguLibrary.Motions
{
    public class Motion
    {
        public string Name = String.Empty;
        public List<MotionBone> Bones;

        public void Read(EndianBinaryReader reader)
        {
            if (!reader.ReadSignature("B3D_ANM", 8))
                throw new Exception("Not a BMM file!");

            reader.SeekCurrent(12);

            bool hasMotion = reader.ReadInt32() == 1;
            bool hasSkin = reader.ReadInt32() == 1;

            reader.SeekCurrent(4);

            Bones = new List<MotionBone>();

            if(reader.ReadSignature("B3D_MTN", 8))
            {
                reader.SeekCurrent(16);

                int boneCount = reader.ReadInt32();

                reader.SeekCurrent(4);

                List<int> frameCountCache = new List<int>();

                for(int i = 0; i < boneCount; i++)
                {
                    MotionBone bone = new MotionBone();

                    bone.Name = reader.ReadString(StringBinaryFormat.FixedLength, 32);

                    frameCountCache.Add(reader.ReadInt32());

                    bone.Keyframes = new Keyframe[frameCountCache.Last()];

                    bone.Index = reader.ReadInt32();

                    reader.SeekCurrent(4);

                    bone.Type = (MotionBoneType)reader.ReadInt32();

                    reader.SeekCurrent(8);

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
                            reader.ReadAtOffset(reader.Position + (16 * frameCountCache[i]), () =>
                            {
                                key.Rotation = reader.ReadVector4();
                            });

                            key.Position = reader.ReadVector3() * 0.08f;
                            //key.Position.Z *= -1.0f;

                            reader.SeekCurrent(4);

                            //Console.WriteLine($"CAMERA POS: {key.Translation} {key.Rotation} {reader.Position} {}");
                        }

                        Bones[i].Keyframes[j] = key;
                    }

                    // Jump the rotation data
                    if (Bones[i].Type == MotionBoneType.RotTrans)
                    {
                        Console.WriteLine($"BMM position: {reader.Position}");
                        reader.SeekCurrent(16 * frameCountCache[i]);
                        Console.WriteLine($"BMM position 02: {reader.Position}");
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
