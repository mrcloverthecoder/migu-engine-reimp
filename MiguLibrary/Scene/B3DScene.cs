using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MiguLibrary.IO;
using MiguLibrary.Objects;
using MiguLibrary.Motions;

namespace MiguLibrary.Scene
{
    public class B3DScene
    {
        public Vector4 BackgroundColor;
        public int EndFrame;
        public ObjectEntry[] Entries;

        public string CameraName;
        public SceneAnimationCut[] CameraCuts;

        public Entry ReadEntry(EndianBinaryReader reader)
        {
            Entry entry = new Entry();

            entry.Name = reader.ReadString(StringBinaryFormat.FixedLength, 0x40).ToUpper();

            // Seek two bytes foward to remove the starting >\ from the string
            reader.SeekCurrent(0x02);
            string pathBase = reader.ReadString(StringBinaryFormat.FixedLength, 0x80 - 0x02);

            entry.Filename = reader.ReadString(StringBinaryFormat.FixedLength, 0x40).ToUpper();

            entry.Path = (pathBase + entry.Filename).Replace("\\", "/").ToUpper();

            return entry;
        }

        public void Read(EndianBinaryReader reader)
        {
            if (!reader.ReadSignature("B3DSCENE", 8))
                throw new Exception("Not a BSX file!");

            reader.SeekBegin(0x40);

            BackgroundColor = reader.ReadColor(ColorFormat.BGRA).AsVector4();
            EndFrame = reader.ReadInt32();

            reader.SeekBegin(0x150);

            reader.SeekCurrent(0x0C);
            int objectCount = reader.ReadInt32();

            Entries = new ObjectEntry[objectCount];

            for(int i = 0; i < objectCount; i++)
            {
                ObjectEntry entry = new ObjectEntry();

                entry.FileEntry = ReadEntry(reader);

                // Skip unknown data
                reader.SeekCurrent(0xC8);

                int animationCutCount = reader.ReadInt32();

                entry.Cuts = new SceneAnimationCut[animationCutCount];
                for(int j = 0; j < animationCutCount; j++)
                {
                    entry.Cuts[j] = SceneAnimationCut.Read(reader);
                }

                int motionCount = reader.ReadInt32();

                entry.Motions = new MotionFileEntry[motionCount];
                for(int j = 0; j < motionCount; j++)
                {
                    Entry e = ReadEntry(reader);
                    entry.Motions[j] = new MotionFileEntry();
                    entry.Motions[j].Name = e.Name;
                    entry.Motions[j].Filename = e.Filename;
                    entry.Motions[j].Path = e.Path;

                    entry.Motions[j].Duration = reader.ReadInt32();
                    reader.SeekCurrent(4);
                    entry.Motions[j].Type = (MotionType)reader.ReadInt32();

                    reader.SeekCurrent(0x38);

                    int motionSubstructCount = reader.ReadInt32();
                    entry.Motions[j].FrameSettings = new FrameSetting[motionSubstructCount];

                    for (int k = 0; k < motionSubstructCount; k++)
                    {
                        entry.Motions[j].FrameSettings[k].Field0 = reader.ReadInt32();
                        entry.Motions[j].FrameSettings[k].PlayCount = reader.ReadInt32();
                        entry.Motions[j].FrameSettings[k].Field8 = reader.ReadInt32();
                        entry.Motions[j].FrameSettings[k].EndFrame = reader.ReadInt32();
                    }
                }

                Entries[i] = entry;
            }

            // If everything is alright we should now be where the camera data is located
            // Unk
            reader.SeekCurrent(0x04);
            // Camera count
            int camCount = reader.ReadInt32();

            // This only supports one camera
            // The way it's read if theres more than 1 camera the last camera will overlap all of the previous

            // First ensure CameraCuts is not null if there are no cameras
            CameraCuts = new SceneAnimationCut[0];

            // Now read the data
            for(int cam = 0; cam < camCount; cam++)
            {
                CameraName = reader.ReadString(StringBinaryFormat.FixedLength, 0x40);

                int camCutCount = reader.ReadInt32();
                CameraCuts = new SceneAnimationCut[camCutCount];
                for (int i = 0; i < camCutCount; i++)
                    CameraCuts[i] = SceneAnimationCut.Read(reader);
            }
        }

        public static B3DScene FromFile(string path)
        {
            B3DScene bsx = new B3DScene();

            using(var reader = new EndianBinaryReader(File.Open(path, FileMode.Open), Encoding.GetEncoding(932), Endianness.LittleEndian))
            {
                bsx.Read(reader);
            }

            return bsx;
        }
    }

    public struct ObjectEntry
    {
        public Entry FileEntry;
        public SceneAnimationCut[] Cuts;
        public MotionFileEntry[] Motions;
    }

    public struct Entry
    {
        public string Name;
        public string Path;
        public string Filename;
    }

    public struct MotionFileEntry
    {
        public string Name;
        public string Path;
        public string Filename;
        public int Duration;
        public MotionType Type;
        public FrameSetting[] FrameSettings;
    }

    public enum MotionType
    {
        Object,
        Body,
        Physics
    }

    public struct FrameSetting
    {
        public int Field0;
        // How many times to play the motion/loop
        public int PlayCount;
        public int Field8;
        public int EndFrame;
    }
}
