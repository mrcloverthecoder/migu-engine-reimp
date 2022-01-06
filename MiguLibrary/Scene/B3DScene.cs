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
        public ObjectEntry[] Entries;

        public Entry ReadEntry(EndianBinaryReader reader)
        {
            Entry entry = new Entry();

            entry.Name = reader.ReadString(StringBinaryFormat.FixedLength, 0x40).ToUpper();

            // Trick to gather the actual path as its stored weirdly in the bsx
            reader.SeekCurrent(0x02);
            string pathBase = reader.ReadString(StringBinaryFormat.NullTerminated);
            string filename = reader.ReadString(StringBinaryFormat.NullTerminated);

            entry.Path = (pathBase + Path.GetFileName(filename)).Replace("\\", "/").ToUpper();

            int totalLength = pathBase.Length + filename.Length + 0x04;
            reader.SeekCurrent(0x80 - totalLength);
            Console.WriteLine($"BSX pos 2: {reader.Position}");

            entry.Filename = reader.ReadString(StringBinaryFormat.FixedLength, 0x40).ToUpper();

            return entry;
        }

        public void Read(EndianBinaryReader reader)
        {
            if (!reader.ReadSignature("B3DSCENE", 8))
                throw new Exception("Not a BSX file!");

            reader.SeekBegin(0x40);

            BackgroundColor = reader.ReadColor(ColorFormat.BGRA).AsVector4();

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

                Console.WriteLine("AAA");
                Console.WriteLine(reader.Position);
                Console.WriteLine(animationCutCount);

                entry.Cuts = new SceneAnimationCut[animationCutCount];
                for(int j = 0; j < animationCutCount; j++)
                {
                    Console.WriteLine("Anim cut read");
                    entry.Cuts[j] = SceneAnimationCut.Read(reader);
                }

                int motionCount = reader.ReadInt32();

                entry.Motions = new Entry[motionCount];
                for(int j = 0; j < motionCount; j++)
                {
                    entry.Motions[j] = ReadEntry(reader);
                    //reader.SeekCurrent(0x58);
                    reader.SeekCurrent(0x44);

                    int motionSubstructCount = reader.ReadInt32();
                    entry.Motions[j].FrameSettings = new FrameSetting[motionSubstructCount];

                    for (int k = 0; k < motionSubstructCount; k++)
                    {
                        entry.Motions[j].FrameSettings[k].Field0 = reader.ReadInt32();
                        entry.Motions[j].FrameSettings[k].PlayCount = reader.ReadInt32();
                        entry.Motions[j].FrameSettings[k].Field8 = reader.ReadInt32();
                        entry.Motions[j].FrameSettings[k].Duration = reader.ReadInt32();
                    }
                }

                Entries[i] = entry;
            }

            Console.WriteLine($"BSX POS: {reader.Position}");
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
        public Entry[] Motions;
    }

    public struct Entry
    {
        public string Name;
        public string Path;
        public string Filename;
        public FrameSetting[] FrameSettings;
    }

    public struct FrameSetting
    {
        public int Field0;
        public int PlayCount;
        public int Field8;
        public int Duration;
    }
}
